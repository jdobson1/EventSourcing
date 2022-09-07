using System.Net;
using System.Reflection;
using Core;
using EventStore;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Projections;

namespace Subscriptions
{
    public class SubscriptionEngine : ISubscriptionEngine
    {
        private readonly IEventTypeResolver _eventTypeResolver;
        private readonly string _endpointUrl;
        private readonly string _authorizationKey;
        private readonly string _databaseId;
        private readonly List<Subscription> _eventSubscriptions = new();
        private SubscriptionCheckpoint _subscriptionCheckpoint;
        private readonly string _instanceName;
        private string _subscriptionCheckpointContainerId;
        private string _subscriptionCheckpointItemId;
        private CosmosClient _client;

        public SubscriptionEngine(IEventTypeResolver eventTypeResolver, string endpointUrl, string instanceName,
            string authorizationKey, string databaseId)
        {
            _eventTypeResolver = eventTypeResolver;
            _endpointUrl = endpointUrl;
            _authorizationKey = authorizationKey;
            _databaseId = databaseId;
            _instanceName = instanceName;
        }

        public async Task HandleChangesAsync(IReadOnlyCollection<Change> changes)
        {
            foreach (var change in changes)
            {
                var @event = change.GetEvent(_eventTypeResolver);

                // use reflection to find event handlers for the event

                var subscriptionsForEvent = _eventSubscriptions
                    .Where(subscription => Type.GetType(subscription.EventType) == @event.GetType());

                foreach (var subscription in subscriptionsForEvent)
                {
                    // var viewName = projection.GetViewName(change.StreamInfo.Id, @event);

                    var handled = false;
                    while (!handled)
                    {
                        //var view = await _viewRepository.LoadViewAsync(viewName);

                        // Only update if the LSN of the change is higher than the view. This will ensure
                        // that changes are only processed once.
                        // NOTE: This only works if there's just a single physical partition in Cosmos DB.
                        // TODO: To support multiple partitions we need access to the leases to store
                        // a LSN per lease in the view. This is not yet possible in the V3 SDK.
                        if (_subscriptionCheckpoint.IsNewerThanCheckpoint(change))
                        {
                            //invoke event handler with event
                            var handler = Type.GetType(subscription.EventHandlerType);
                            MethodInfo method = handler.GetMethod("Handle");
                            MethodInfo genericMethod = method.MakeGenericMethod(new Type[] { typeof(IEvent) });
                            genericMethod.Invoke(Activator.CreateInstance(handler), new object[] { @event });

                            handled = await SaveCheckpointAsync();
                        }
                        else
                        {
                            // Already handled.
                            handled = true;
                        }

                        if (!handled)
                            await Task.Delay(100);
                    }
                }
            }
        }

        public void Subscribe(Subscription subscription)
        {
            _eventSubscriptions.Add(subscription);
        }

        public async Task<bool> SaveCheckpointAsync()
        {
            var container = _client.GetContainer(_databaseId, _subscriptionCheckpointContainerId);
            var partitionKey = new PartitionKey(_subscriptionCheckpointItemId);

            try
            {
                await container.UpsertItemAsync(_subscriptionCheckpoint, partitionKey);

                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return false;
            }
        }

        private async Task LoadSubscriptionCheckpoint()
        {
            Container subscriptionCheckpointContainer = _client.GetContainer(_databaseId, _subscriptionCheckpointContainerId);
            var q = subscriptionCheckpointContainer.GetItemLinqQueryable<SubscriptionCheckpoint>();
            var iterator = q.ToFeedIterator();
            var results = await iterator.ReadNextAsync();
            var subscriptionCheckpoints = results.ToList();

            //_subscriptionCheckpoint = await subscriptionCheckpointContainer.ReadItemAsync<SubscriptionCheckpoint>(_subscriptionCheckpointItemId, new PartitionKey(_subscriptionCheckpointItemId));

            if (!subscriptionCheckpoints.Any())
            {
                var item = await subscriptionCheckpointContainer.UpsertItemAsync(new SubscriptionCheckpoint { Id = _subscriptionCheckpointItemId, LogicalSequenceNumber = 0 }, new PartitionKey(_subscriptionCheckpointItemId));
                _subscriptionCheckpoint = item.Resource;
            }
        }

        private async Task MigrateDatabase()
        {
            _client = new CosmosClient(_endpointUrl, _authorizationKey, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway
            });

            Database database = _client.GetDatabase(_databaseId);

            _subscriptionCheckpointContainerId = $"sub-ckpt-{_instanceName}";
            _subscriptionCheckpointItemId = $"sub_ckpt-{_instanceName}-id";
            await database.DefineContainer(_subscriptionCheckpointContainerId, "/id").CreateIfNotExistsAsync();
        }

        public async Task Initialize()
        {
            await MigrateDatabase();
            await LoadSubscriptionCheckpoint();
        }
    }

    public class CosmosSubscriptionEngine : ICfpSubscriptionEngine
    {
        private readonly IEventTypeResolver _eventTypeResolver;
        private readonly string _endpointUrl;
        private readonly string _authorizationKey;
        private readonly string _databaseId;
        private string _subscriptionContainerId;
        private string _subscriptionCheckpointContainerId;
        private string _subscriptionCheckpointItemId;
        private readonly string _eventContainerId;
        private readonly string _leaseContainerId;
        private string _instanceName;
        private List<Subscription> _eventSubscriptions;
        private ChangeFeedProcessor _changeFeedProcessor;
        private SubscriptionCheckpoint _subscriptionCheckpoint;
        private CosmosClient _client;

        public CosmosSubscriptionEngine(IEventTypeResolver eventTypeResolver, string endpointUrl, string instanceName,
            string authorizationKey, string databaseId, string eventContainerId = "events", string leaseContainerId = "leases")
        {
            _eventTypeResolver = eventTypeResolver;
            _instanceName = instanceName;
            _endpointUrl = endpointUrl;
            _authorizationKey = authorizationKey;
            _databaseId = databaseId;
            _eventContainerId = eventContainerId;
            _leaseContainerId = leaseContainerId;
            _eventSubscriptions = new List<Subscription>();
        }

        public async Task StartAsync()
        {
            await MigrateDatabase();
            await LoadSubscriptions();
            await LoadSubscriptionCheckpoint();

            Container eventContainer = _client.GetContainer(_databaseId, _eventContainerId);
            Container leaseContainer = _client.GetContainer(_databaseId, _leaseContainerId);
           
            _changeFeedProcessor = eventContainer
                .GetChangeFeedProcessorBuilder<Change>("Subscription", HandleChangesAsync)
                .WithInstanceName(_instanceName)
                .WithLeaseContainer(leaseContainer)
                .WithStartTime(DateTime.MinValue.ToUniversalTime())
                .Build();

            await _changeFeedProcessor.StartAsync();
        }

        public Task StopAsync()
        {
            return _changeFeedProcessor.StopAsync();
        }

        public void Subscribe(Subscription subscription)
        {
            _eventSubscriptions.Add(subscription);
        }

        private async Task MigrateDatabase()
        {
            _client = new CosmosClient(_endpointUrl, _authorizationKey, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway
            });

            Database database = _client.GetDatabase(_databaseId);

            _subscriptionContainerId = $"sub-{_instanceName}";
            _subscriptionCheckpointContainerId = $"sub-ckpt-{_instanceName}";
            _subscriptionCheckpointItemId = $"sub_ckpt-{_instanceName}-id";
            await database.DefineContainer(_subscriptionCheckpointContainerId, "/id").CreateIfNotExistsAsync();
            await database.DefineContainer(_subscriptionContainerId, "/id").CreateIfNotExistsAsync();
        }

        private async Task HandleChangesAsync(IReadOnlyCollection<Change> changes, CancellationToken cancellationToken)
        {
            foreach (var change in changes)
            {
                var @event = change.GetEvent(_eventTypeResolver);

                // use reflection to find event handlers for the event

                var subscriptionsForEvent = _eventSubscriptions
                    .Where(subscription => Type.GetType(subscription.EventType) == @event.GetType());
                
                foreach (var subscription in subscriptionsForEvent)
                {
                   // var viewName = projection.GetViewName(change.StreamInfo.Id, @event);

                    var handled = false;
                    while (!handled)
                    {
                        //var view = await _viewRepository.LoadViewAsync(viewName);

                        // Only update if the LSN of the change is higher than the view. This will ensure
                        // that changes are only processed once.
                        // NOTE: This only works if there's just a single physical partition in Cosmos DB.
                        // TODO: To support multiple partitions we need access to the leases to store
                        // a LSN per lease in the view. This is not yet possible in the V3 SDK.
                        if (_subscriptionCheckpoint.IsNewerThanCheckpoint(change))
                        {
                            //invoke event handler with event
                            var handler = Type.GetType(subscription.EventHandlerType);
                            MethodInfo method = handler.GetMethod("Handle");
                            MethodInfo genericMethod = method.MakeGenericMethod(new Type[] { typeof(IEvent) });
                            genericMethod.Invoke(Activator.CreateInstance(handler), new object[] { @event });

                            handled = await SaveCheckpointAsync();
                        }
                        else
                        {
                            // Already handled.
                            handled = true;
                        }

                        if (!handled)
                            await Task.Delay(100);
                    }
                }
            }
        }

        public async Task<bool> SaveSubscriptionAsync(Subscription subscription)
        {
            var container = _client.GetContainer(_databaseId, _subscriptionContainerId);
            var id = $"{Type.GetType(subscription.EventType).Name}:{Type.GetType(subscription.EventHandlerType).Name}";
            var partitionKey = new PartitionKey(id);

            var item = new
            {
                id = id,
                eventType = subscription.EventType,
                eventHandlerType = subscription.EventHandlerType
            };

            try
            {
                await container.UpsertItemAsync(item, partitionKey);

                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return false;
            }
        }

        public async Task<bool> SaveCheckpointAsync()
        {
            var container = _client.GetContainer(_databaseId, _subscriptionCheckpointContainerId);
            var partitionKey = new PartitionKey(_subscriptionCheckpointItemId);

            //var item = new
            //{
            //    id = name,
            //    logicalCheckpoint = view.LogicalCheckpoint,
            //    payload = view.Payload
            //};

            try
            {
                await container.UpsertItemAsync(_subscriptionCheckpoint, partitionKey);

                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return false;
            }
        }

        private async Task LoadSubscriptions()
        {
            foreach (var subscription in _eventSubscriptions)
                await SaveSubscriptionAsync(subscription);

            var container = _client.GetContainer(_databaseId, _subscriptionContainerId);
            var q = container.GetItemLinqQueryable<Subscription>();
            var iterator = q.ToFeedIterator();
            var results = await iterator.ReadNextAsync();
            _eventSubscriptions = results.ToList();
        }

        private async Task LoadSubscriptionCheckpoint()
        {
            Container subscriptionCheckpointContainer = _client.GetContainer(_databaseId, _subscriptionCheckpointContainerId);
            var q = subscriptionCheckpointContainer.GetItemLinqQueryable<SubscriptionCheckpoint>();
            var iterator = q.ToFeedIterator();
            var results = await iterator.ReadNextAsync();
            var subscriptionCheckpoints = results.ToList();

            //_subscriptionCheckpoint = await subscriptionCheckpointContainer.ReadItemAsync<SubscriptionCheckpoint>(_subscriptionCheckpointItemId, new PartitionKey(_subscriptionCheckpointItemId));

            if (!subscriptionCheckpoints.Any())
            {
                var item = await subscriptionCheckpointContainer.UpsertItemAsync(new SubscriptionCheckpoint { Id = _subscriptionCheckpointItemId, LogicalSequenceNumber = 0 }, new PartitionKey(_subscriptionCheckpointItemId));
                _subscriptionCheckpoint = item.Resource;
            }
        }
    }
}