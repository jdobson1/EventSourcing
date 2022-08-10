using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Projections
{
    public class CosmosViewRepository : IViewRepository
    {
        protected readonly CosmosClient Client;
        protected readonly string DatabaseId;
        protected readonly string ContainerId;

        public CosmosViewRepository(string endpointUrl, string authorizationKey, string databaseId,
            string containerId = "views")
        {
            Client = new CosmosClient(endpointUrl, authorizationKey, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway
            });
            DatabaseId = databaseId;
            ContainerId = containerId;
        }

        public async Task<View> LoadViewAsync(string name)
        {
            var container = Client.GetContainer(DatabaseId, ContainerId);
            var partitionKey = new PartitionKey(name);

            try
            {
                var response = await container.ReadItemAsync<View>(name, partitionKey);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return new View();
            }
        }

        public async Task<TView> LoadViewAsync<TView>(string name) where TView : new()
        {
            var container = Client.GetContainer(DatabaseId, ContainerId);
            var partitionKey = new PartitionKey(name);

            try
            {
                var response = await container.ReadItemAsync<TView>(name, partitionKey);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return new TView();
            }
        }

        public async Task<bool> SaveViewAsync(string name, View view)
        {
            var container = Client.GetContainer(DatabaseId, ContainerId);
            var partitionKey = new PartitionKey(name);

            var item = new 
            {
                id = name,
                logicalCheckpoint = view.LogicalCheckpoint,
                payload = view.Payload
            };

            try
            {
                await container.UpsertItemAsync(item, partitionKey, new ItemRequestOptions
                {
                    IfMatchEtag = view.Etag
                });

                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return false;
            }
        }
    }
}