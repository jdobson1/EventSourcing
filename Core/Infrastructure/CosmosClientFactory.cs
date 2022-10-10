using Microsoft.Azure.Cosmos;

namespace Core.Infrastructure;

public class CosmosClientFactory : ICosmosClientFactory
{
    public CosmosClient? Create(string endpointUrl, string authorizationKey, CosmosClientOptions cosmosClientOptions)
    {
        return new CosmosClient(endpointUrl, authorizationKey, cosmosClientOptions);
    }

    public CosmosClient CreateWithToken(string endpointUrl, string resourceToken, CosmosClientOptions cosmosClientOptions)
    {
        return new CosmosClient(accountEndpoint: endpointUrl, authKeyOrResourceToken: resourceToken, cosmosClientOptions);
    }

    public CosmosClient CreateForUser(string endpointUrl, string authorizationKey, string userId, string streamId, string databaseId,
        CosmosClientOptions cosmosClientOptions)
    {
        var client = Create(endpointUrl, authorizationKey, cosmosClientOptions);
        Database database = client.GetDatabase(databaseId);
        var user = database.GetUser(userId);
        Permission permission = user.GetPermission($"permission_{userId}_{streamId}");
        var resourceToken = permission.ReadAsync().Result.Resource.Token;
        return CreateWithToken(endpointUrl, resourceToken, cosmosClientOptions);
    }
}