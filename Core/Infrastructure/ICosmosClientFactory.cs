﻿using Microsoft.Azure.Cosmos;

namespace Core.Infrastructure
{
    public interface ICosmosClientFactory
    {
        CosmosClient? Create(string endpointUrl, string authorizationKey, CosmosClientOptions cosmosClientOptions);
        CosmosClient CreateWithToken(string endpointUrl, string resourceToken, CosmosClientOptions cosmosClientOptions);
        CosmosClient CreateForUser(string endpointUrl, string authorizationKey, string userId, string streamId, string databaseId, CosmosClientOptions cosmosClientOptions);
    }
}
