using System.Net;
using Core.Infrastructure;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace Sagas.StateProviders;

public class CosmosStateProvider : ISagaStateProvider
{
    private readonly ICosmosClientFactory _cosmosClientFactory;
    private readonly CosmosDatabaseContext _cosmosDatabaseContext;

    public CosmosStateProvider(ICosmosClientFactory cosmosClientFactory, CosmosDatabaseContext cosmosDatabaseContext)
    {
        _cosmosClientFactory = cosmosClientFactory;
        _cosmosDatabaseContext = cosmosDatabaseContext;
    }

    public async Task DeleteAsync<TSagaState>(Guid id)
    {
        var client = _cosmosClientFactory.Create(_cosmosDatabaseContext.EndpointUrl, _cosmosDatabaseContext.AuthorizationKey, new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway });
        var container = client?.GetContainer(_cosmosDatabaseContext.DatabaseId, _cosmosDatabaseContext.ContainerId);
        var partitionKey = new PartitionKey(id.ToString());
        
        await container?.DeleteItemAsync<TSagaState>(id.ToString(), partitionKey)!;
    }

    public async Task<TSagaState> GetAsync<TSagaState>(Guid id) where TSagaState : class, ISagaState, new()
    {
        try
        {
            var client = _cosmosClientFactory.Create(_cosmosDatabaseContext.EndpointUrl, _cosmosDatabaseContext.AuthorizationKey, new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway });
            var container = client?.GetContainer(_cosmosDatabaseContext.DatabaseId, _cosmosDatabaseContext.ContainerId);
            var partitionKey = new PartitionKey(id.ToString());

            var response = await container?.ReadItemAsync<SagaStateWrapper>(id.ToString(), partitionKey)!;
            return response.Resource.Payload.ToObject<TSagaState>();
        }
        catch (Exception ex)
        {
            return new TSagaState();
        }
       
    }

    public async Task<bool> SaveAsync(Guid id, ISagaState sagaState)
    {
        var client = _cosmosClientFactory.Create(_cosmosDatabaseContext.EndpointUrl, _cosmosDatabaseContext.AuthorizationKey, new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway });
        var container = client?.GetContainer(_cosmosDatabaseContext.DatabaseId, _cosmosDatabaseContext.ContainerId);
        var partitionKey = new PartitionKey(id.ToString());

        sagaState.Id = id;

        var item = new
        {
            id = id,
            payload = JObject.FromObject(sagaState)
        };
        
        await container?.UpsertItemAsync(item, partitionKey)!;

        return true;
    }
}