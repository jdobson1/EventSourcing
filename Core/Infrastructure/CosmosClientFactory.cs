using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Core.Infrastructure;

public class CosmosClientFactory : ICosmosClientFactory
{
    private readonly ICosmosDatabaseUserManager _userManager;

    public CosmosClientFactory(ICosmosDatabaseUserManager userManager)
    {
        _userManager = userManager;
    }

    public CosmosClient? Create(string endpointUrl, string authorizationKey, CosmosClientOptions cosmosClientOptions)
    {
        return new CosmosClient(endpointUrl, authorizationKey, cosmosClientOptions);
    }

    public CosmosClient CreateWithToken(string endpointUrl, string resourceToken, CosmosClientOptions cosmosClientOptions)
    {
        return new CosmosClient(accountEndpoint: endpointUrl, authKeyOrResourceToken: resourceToken, cosmosClientOptions);
    }

    public async Task<CosmosClient> CreateForUser(string endpointUrl, string authorizationKey, string userId, string streamId, string databaseId, string containerId,
        CosmosClientOptions cosmosClientOptions)
    {
        if (string.IsNullOrEmpty(containerId)) throw new ArgumentNullException(nameof(containerId));
        if (string.IsNullOrEmpty(databaseId)) throw new ArgumentNullException(nameof(databaseId));
        if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
        if (string.IsNullOrEmpty(streamId)) throw new ArgumentNullException(nameof(streamId));
        if (string.IsNullOrEmpty(endpointUrl)) throw new ArgumentNullException(nameof(endpointUrl));
        if (string.IsNullOrEmpty(authorizationKey)) throw new ArgumentNullException(nameof(authorizationKey));

        var token = await _userManager.GetResourceTokenAsync(userId, $"permission_{streamId}", streamId);

        if (token == null)
            throw new Exception(
                $"CosmosDB resource token for user: {userId} and stream: {streamId} does not exist or could not be retrieved");

        return CreateWithToken(endpointUrl, token, cosmosClientOptions);
    }
}