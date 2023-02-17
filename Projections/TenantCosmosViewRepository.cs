using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure.Core;
using Core.Infrastructure;
using Microsoft.Azure.Cosmos;
using Container = Microsoft.Azure.Cosmos.Container;
using Database = Microsoft.Azure.Cosmos.Database;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;
using Permission = Microsoft.Azure.Cosmos.Permission;
using PermissionMode = Microsoft.Azure.Cosmos.PermissionMode;
using User = Microsoft.Azure.Cosmos.User;

namespace Projections;

/// <summary>
/// A multi-tenant view repository for CosmosDB.
/// </summary>
public class TenantCosmosViewRepository : ITenantViewRepository
{
    protected readonly string AuthorizationKey;
    protected readonly string DatabaseId;
    protected readonly string EndpointUrl;
    protected readonly string ContainerId;
    private readonly ICosmosClientFactory _cosmosClientFactory;

    public TenantCosmosViewRepository(string endpointUrl, string authorizationKey, string databaseId,
        ICosmosClientFactory cosmosClientFactory,
        string containerId = "views")
    {
        EndpointUrl = endpointUrl;
        AuthorizationKey = authorizationKey;
        DatabaseId = databaseId;
        _cosmosClientFactory = cosmosClientFactory;
        ContainerId = containerId;
    }

    public async Task<View> LoadViewAsync(Guid clientId, string name)
    {
        try
        {
            var container = await GetContainerAsync(clientId, name);
            var partitionKey = new PartitionKey(name);

            var response = await container.ReadItemAsync<View>(name, partitionKey);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
        {
            return new View();
        }
        catch (Exception ex)
        {
            return new View();
        }
    }

    public async Task<TView> LoadViewAsync<TView>(Guid clientId, string name) where TView : new()
    {
        try
        {
            var container = await GetContainerAsync(clientId, name);
            var partitionKey = new PartitionKey(name);

            var response = await container.ReadItemAsync<TView>(name, partitionKey);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return new TView();
        }
    }

    public async Task<bool> SaveViewAsync(Guid clientId, string name, View view)
    {
        try
        {
            var client = _cosmosClientFactory.Create(EndpointUrl, AuthorizationKey, new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway });
            var database = client?.GetDatabase(DatabaseId);
            var container = client?.GetContainer(DatabaseId, ContainerId);
            var partitionKey = new PartitionKey(name);

            var item = new
            {
                id = name,
                logicalCheckpoint = view.LogicalCheckpoint,
                payload = view.Payload
            };

            await container?.UpsertItemAsync(item, partitionKey, new ItemRequestOptions
            {
                IfMatchEtag = view.Etag
            })!;

            await CreateUserAndPermissionsAsync(clientId.ToString(), name, database, container);

            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            return false;
        }
    }

    private async Task<Container> GetContainerAsync(Guid clientId, string streamId)
    {
        var client = _cosmosClientFactory.CreateForUser(EndpointUrl, AuthorizationKey, clientId.ToString(),
            streamId, DatabaseId, new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway });
        return client.GetContainer(DatabaseId, ContainerId);
       
    }

    private async Task CreateUserAndPermissionsAsync(string userId, string partitionKey, Database database, Container container)
    {
        var user = await CreateUserAsync(userId, database);
        await CreatePermissionAsync(user, partitionKey, container);
    }

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