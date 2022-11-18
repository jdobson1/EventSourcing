using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Core.Domain;
using Core.Infrastructure;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Database = Microsoft.Azure.Cosmos.Database;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;
using Permission = Microsoft.Azure.Cosmos.Permission;
using PermissionMode = Microsoft.Azure.Cosmos.PermissionMode;
using User = Microsoft.Azure.Cosmos.User;

namespace EventStore
{
    public class CosmosEventStore : IEventStore
    {
        private readonly ICosmosClientFactory _cosmosClientFactory;
        private readonly IEventTypeResolver _eventTypeResolver;
        private readonly string _databaseId;
        private readonly string _containerId;
        private readonly string _endpointUrl;
        private readonly string _authorizationKey;
        private readonly CosmosClient _mainClient;
        private readonly string _platformDomain;

        public CosmosEventStore(IEventTypeResolver eventTypeResolver, string endpointUrl, string authorizationKey,
            string databaseId, ICosmosClientFactory cosmosClientFactory, string platformDomain, string containerId = "events")
        {
            _eventTypeResolver = eventTypeResolver;
            _endpointUrl = endpointUrl;
            _authorizationKey = authorizationKey;
            _databaseId = databaseId;
            _cosmosClientFactory = cosmosClientFactory;
            _containerId = containerId;
            _platformDomain = platformDomain;
            _mainClient = _cosmosClientFactory.Create(_endpointUrl, _authorizationKey,
                new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway });
        }

        public async Task<EventStream> LoadStreamAsync(string clientId, string streamId)
        {
            var client = _cosmosClientFactory.CreateForUser(_endpointUrl, _authorizationKey, clientId, streamId, _databaseId,
                new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway});
            var container = client.GetContainer(_databaseId, _containerId);

            var sqlQueryText = "SELECT * FROM events e"
                + " WHERE e.stream.id = @streamId"
                + " ORDER BY e.stream.version"; 

            var queryDefinition = new QueryDefinition(sqlQueryText)
                .WithParameter("@streamId", streamId);

            int version = 0;
            var events = new List<IEvent>();

            var feedIterator = container.GetItemQueryIterator<EventWrapper>(queryDefinition,
                requestOptions: new QueryRequestOptions() { PartitionKey = new PartitionKey(streamId) });

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<EventWrapper> response = await feedIterator.ReadNextAsync();
                foreach (var eventWrapper in response)
                {
                    version = eventWrapper.StreamInfo.Version;

                    events.Add(eventWrapper.GetEvent(_eventTypeResolver));
                }
            }

            return new EventStream(streamId, version, events);
        }

        public async Task<EventStream> LoadStreamAsync(string clientId, string streamId, int fromVersion)
        {
            var client = _cosmosClientFactory.CreateForUser(_endpointUrl, _authorizationKey, clientId, streamId, _databaseId,
                new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway });
            var container = client.GetContainer(_databaseId, _containerId);

            var sqlQueryText = "SELECT * FROM events e"
                + " WHERE e.stream.id = @streamId AND e.stream.version >= @fromVersion"
                + " ORDER BY e.stream.version"; 

            var queryDefinition = new QueryDefinition(sqlQueryText)
                .WithParameter("@streamId", streamId)
                .WithParameter("@fromVersion", fromVersion);

            int version = 0;
            var events = new List<IEvent>();

            var feedIterator = container.GetItemQueryIterator<EventWrapper>(queryDefinition,
                requestOptions: new QueryRequestOptions() { PartitionKey = new PartitionKey(streamId) });

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<EventWrapper> response = await feedIterator.ReadNextAsync();
                foreach (var eventWrapper in response)
                {
                    version = eventWrapper.StreamInfo.Version;

                    events.Add(eventWrapper.GetEvent(_eventTypeResolver));
                }
            }

            return new EventStream(streamId, version, events);
        }

        public async Task<EventStream> LoadStreamAsync(string indexedPropertyValue)
        {
            var container = _mainClient.GetContainer(_databaseId, _containerId);

            var sqlQueryText = "SELECT * FROM events e"
                               + " WHERE e.stream.index = @indexedPropertyValue"
                               + " ORDER BY e.stream.version";

            var queryDefinition = new QueryDefinition(sqlQueryText)
                .WithParameter("@indexedPropertyValue", indexedPropertyValue);

            int version = 0;
            var events = new List<IEvent>();
            var streamId = string.Empty;

            var feedIterator = container.GetItemQueryIterator<EventWrapper>(queryDefinition);

            while (feedIterator.HasMoreResults)
            {
                FeedResponse<EventWrapper> response = await feedIterator.ReadNextAsync();
                foreach (var eventWrapper in response)
                {
                    version = eventWrapper.StreamInfo.Version;
                    streamId = eventWrapper.StreamInfo.Id;

                    events.Add(eventWrapper.GetEvent(_eventTypeResolver));
                }
            }

            return new EventStream(streamId, version, events);
        }

        public async Task<bool> AppendToStreamAsync(string clientId, string streamId, int expectedVersion, IEnumerable<IEvent> events)
        {
            var container = _mainClient.GetContainer(_databaseId, _containerId);

            var partitionKey = new PartitionKey(streamId);

            dynamic[] parameters = new dynamic[]
            {
                streamId,
                expectedVersion,
                SerializeEvents(streamId, expectedVersion, events)
            };

            var result = await container.Scripts.ExecuteStoredProcedureAsync<bool>("spAppendToStream", partitionKey, parameters);

            if (!result) return result;
            
            var database = _mainClient.GetDatabase(_databaseId);
            var cosmosDbUser = await CreateUserAsync(clientId, database);
            await CreatePermissionAsync(cosmosDbUser, streamId, container);
           
            return result;
        }

        private string SerializeEvents(string streamId, int expectedVersion, IEnumerable<IEvent> events)
        {
            var items = events.Select(e => new EventWrapper
            {
                Id = $"{streamId}:{++expectedVersion}",
                StreamInfo = new StreamInfo
                {
                    Id = streamId,
                    Version = expectedVersion,
                    Index = GetIndexedProperty(e)
                },
                EventType = e.GetType().Name,
                EventData = JObject.FromObject(e),
                PlatformDomain = _platformDomain
            });

            return JsonConvert.SerializeObject(items);
        }

        private static string GetIndexedProperty(IEvent @event)
        {
            var prop = @event.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.GetCustomAttributes(typeof(Index), false).Count() == 1);
            return prop?.GetValue(@event, null)?.ToString();
        }

        #region Snapshot Functionality

        private async Task<TSnapshot> LoadSnapshotAsync<TSnapshot>(string clientId, string streamId)
        {
            var client = _cosmosClientFactory.CreateForUser(_endpointUrl, _authorizationKey, clientId, streamId, _databaseId,
                new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway });
            Container container = client.GetContainer(_databaseId, _containerId);

            PartitionKey partitionKey = new PartitionKey(streamId);

            var response = await container.ReadItemAsync<TSnapshot>(streamId, partitionKey);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Resource;
            }

            return default(TSnapshot);
        }

        #endregion

        private async Task<User> CreateUserAsync(string userId, Database database)
        {
            User cosmosDbUser = null;
            var user = database.GetUser(userId);

            try
            {
                var cosmosDbUserResponse = await user.ReadAsync();
                cosmosDbUser = cosmosDbUserResponse.User;
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    cosmosDbUser = await database.UpsertUserAsync(userId);
                }
            }

            return cosmosDbUser;
        }

        private async Task<Permission> CreatePermissionAsync(User user, string partitionKey, Container container)
        {
            var permissionId = $"permission_{user.Id}_{partitionKey}";
            var permission = user?.GetPermission(permissionId);

            try
            {
                await permission?.ReadAsync()!;
            }
            catch (CosmosException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    await user.CreatePermissionAsync(
                        new PermissionProperties(
                            id: permissionId,
                            permissionMode: PermissionMode.All,
                            container: container,
                            resourcePartitionKey: new PartitionKey(partitionKey)), tokenExpiryInSeconds: 18000);
                }
            }

            return permission;
        }
    }
}