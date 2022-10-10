using System.Net;
using Microsoft.Azure.Cosmos;

namespace Core.Infrastructure;

public class CosmosDatabaseUserManager : ICosmosDatabaseUserManager
{
    private readonly ICosmosClientFactory _cosmosClientFactory;
    private CosmosClient? _client;

    public CosmosDatabaseUserManager(ICosmosClientFactory cosmosClientFactory)
    {
        _cosmosClientFactory = cosmosClientFactory;
            
    }

    public async Task<User> CreateUserIfNotExistsAsync(string userId, CosmosDatabaseContext context)
    {
        SetClient(context);

        // Create user or if exists add permission
        var database = _client?.GetDatabase("event-store");

        User? cosmosDbUser = null;

        try
        {
            var user = database?.GetUser(userId);
            var cosmosDbUserResponse = await user?.ReadAsync()!;
            cosmosDbUser = cosmosDbUserResponse.User;
        }
        catch (CosmosException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                cosmosDbUser = await database?.UpsertUserAsync(userId)!;
            }
        }

        return cosmosDbUser;
    }

    public async Task<Permission> CreateUserPermissionIfNotExistsAsync(string userId, string permissionId, string resource, string partitionKey,
        CosmosDatabaseContext context)
    {
        SetClient(context);
        //var permissionId = $"permission_{clientId}_{streamId}";
        var user = await CreateUserIfNotExistsAsync(userId, context);

        var permission = user.GetPermission(permissionId);

        Permission? cosmosPermission = null;

        try
        {
            cosmosPermission = await permission?.ReadAsync()!;
        }
        catch (CosmosException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var container = _client?.GetContainer(context.DatabaseId, context.ContainerId);

                cosmosPermission = await user?.CreatePermissionAsync(
                    new PermissionProperties(
                        id: permissionId,
                        permissionMode: PermissionMode.All,
                        container: container,
                        resourcePartitionKey: new PartitionKey(partitionKey)))!;
            }
        }

        return cosmosPermission;
    }

    private void SetClient(CosmosDatabaseContext context)
    {
        if (_client == null) return;

        _client = _cosmosClientFactory.Create(context.EndpointUrl, context.AuthorizationKey, context.Options);
    }
}