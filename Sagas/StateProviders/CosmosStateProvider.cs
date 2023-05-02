using System.Net;
using Core.Infrastructure;
using Microsoft.Azure.Cosmos;

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

    public async Task<bool> DeleteAsync(Guid correlationId)
    {
        try
        {
            var client = _cosmosClientFactory.Create(_cosmosDatabaseContext.EndpointUrl, _cosmosDatabaseContext.AuthorizationKey, new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway });
            var container = client?.GetContainer(_cosmosDatabaseContext.DatabaseId, _cosmosDatabaseContext.ContainerId);
            var partitionKey = new PartitionKey(correlationId.ToString());
            
            await container?.DeleteItemAsync<CosmosSagaState>(correlationId.ToString(), partitionKey)!;
           
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            return false;
        }
    }

    public async Task<SagaState> GetAsync(Guid correlationId)
    {
        try
        {
            var client = _cosmosClientFactory.Create(_cosmosDatabaseContext.EndpointUrl, _cosmosDatabaseContext.AuthorizationKey, new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway });
            var container = client?.GetContainer(_cosmosDatabaseContext.DatabaseId, _cosmosDatabaseContext.ContainerId);
            var partitionKey = new PartitionKey(correlationId.ToString());

            var response = await container?.ReadItemAsync<CosmosSagaState>(correlationId.ToString(), partitionKey)!;
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
        {
            return new CosmosSagaState();
        }
        catch (Exception ex)
        {
            return new CosmosSagaState();
        }
    }

    public async Task<bool> IsCompletedAsync(Guid correlationId)
    {
        var state = await GetAsync(correlationId);
        return state.Status == SagaStatus.Completed;
    }

    public async Task<bool> SaveAsync(Guid correlationId, SagaState sagaState)
    {
        try
        {
            var client = _cosmosClientFactory.Create(_cosmosDatabaseContext.EndpointUrl, _cosmosDatabaseContext.AuthorizationKey, new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway });
            var container = client?.GetContainer(_cosmosDatabaseContext.DatabaseId, _cosmosDatabaseContext.ContainerId);
            var partitionKey = new PartitionKey(correlationId.ToString());

            var item = new
            {
                correlationId = correlationId,
                payload = sagaState.Payload
            };

            await container?.UpsertItemAsync(item, partitionKey, new ItemRequestOptions
            {
                IfMatchEtag = ((CosmosSagaState)sagaState).Etag
            })!;

            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            return false;
        }
    }
}