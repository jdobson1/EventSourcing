using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Distributed;

namespace Core.Infrastructure;


public class CosmosDatabaseUserManager : ICosmosDatabaseUserManager
{
    private readonly IDistributedCache _cache;
    private readonly CosmosDatabaseContext _cosmosDatabaseContext;
    private CosmosClient? _client;
    private const int TokenGracePeriodSecs = 301;

    public CosmosDatabaseUserManager(IDistributedCache cache, CosmosDatabaseContext cosmosDatabaseContext)
    {
        _cache = cache;
        _cosmosDatabaseContext = cosmosDatabaseContext;
        SetClient();
    }

    /// <inheritdoc cref="ICosmosDatabaseUserManager" />
    public async Task<User> CreateUserIfNotExistsAsync(string userId)
    {
        SetClient();

        User cosmosDbUser = null!;

        try
        {
            cosmosDbUser = await GetUserAsync(userId);
        }
        catch (CosmosException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var database = _client?.GetDatabase(_cosmosDatabaseContext.DatabaseId);
                cosmosDbUser = await database?.UpsertUserAsync(userId)!;
            }
        }

        return cosmosDbUser;
    }

    public async Task<UserResponse> DeleteUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

        var database = _client?.GetDatabase(_cosmosDatabaseContext.DatabaseId);
        var user = database?.GetUser(userId);
        return await user?.DeleteAsync()!;
    }

    /// <inheritdoc cref="ICosmosDatabaseUserManager" />
    public async Task<PermissionResponse> CreateUserPermissionIfNotExistsAsync(string userId, string permissionId, string? resource, string partitionKey)
    {
        SetClient();

        var user = await CreateUserIfNotExistsAsync(userId);
        var permission = user.GetPermission(permissionId);

        PermissionResponse? cosmosPermission = null;

        try
        {
            cosmosPermission = await permission?.ReadAsync()!;
        }
        catch (CosmosException ex)
        {
            if (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var container = _client?.GetContainer(_cosmosDatabaseContext.DatabaseId, _cosmosDatabaseContext.ContainerId);

                cosmosPermission = await user.CreatePermissionAsync(
                    new PermissionProperties(
                        id: permissionId,
                        permissionMode: PermissionMode.All,
                        container: container,
                        resourcePartitionKey: new PartitionKey(partitionKey)), tokenExpiryInSeconds: _cosmosDatabaseContext.ResourceTokenExpirationSecs)!;
            }
        }

        await CacheToken(userId, cosmosPermission?.Resource.Token!);

        return cosmosPermission!;
    }

    public async Task<PermissionResponse> DeleteUserPermissionAsync(string userId, string permissionId)
    {
        SetClient();
        var database = _client?.GetDatabase(_cosmosDatabaseContext.DatabaseId);
        var user = database?.GetUser(userId);
        var permission = user?.GetPermission(permissionId);
        return await permission?.DeleteAsync()!;
    }

    /// <inheritdoc cref="ICosmosDatabaseUserManager" />
    public async Task<string> GetResourceTokenAsync(string userId, string permissionId, string streamId)
    {
        var token = await GetResourceTokenFromCache(userId);

        if (!string.IsNullOrEmpty(token))
            return token;

        var user = await CreateUserIfNotExistsAsync(userId);
        //var permissionIdParts = permissionId.Split('_');
        //var streamId = $"{permissionIdParts.ElementAt(1)}-{permissionIdParts.Last()}";
        var permissionResponse = await CreateUserPermissionIfNotExistsAsync(user.Id, permissionId, null, streamId);
        token = permissionResponse.Resource.Token;

        return token;
    }

    /// <inheritdoc cref="ICosmosDatabaseUserManager" />
    public async Task<bool> UserExists(string userId)
    {
        var user = await GetUserAsync(userId);
        return !string.IsNullOrEmpty(user.Id);
    }

    /// <inheritdoc cref="ICosmosDatabaseUserManager" />
    public async Task<bool> IsResourceTokenCached(string userId)
    {
        return !string.IsNullOrEmpty(await GetResourceTokenFromCache(userId));
    }

    private async Task CacheToken(string userId, string token)
    {
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddSeconds(_cosmosDatabaseContext.ResourceTokenExpirationSecs + TokenGracePeriodSecs), TimeSpan.Zero)
        };

        await _cache.SetStringAsync($"{userId}-{_cosmosDatabaseContext.DatabaseId}-cosmos-token", token, cacheOptions);
    }

    private void SetClient()
    {
        if (_client != null) return;

        _client = new CosmosClient(_cosmosDatabaseContext.EndpointUrl, _cosmosDatabaseContext.AuthorizationKey, _cosmosDatabaseContext.Options);
    }

    private async Task<string> GetResourceTokenFromCache(string userId)
    {
        return await _cache.GetStringAsync($"{userId}-{_cosmosDatabaseContext.DatabaseId}-cosmos-token");
    }

    public async Task<User> GetUserAsync(string userId)
    {
        var database = _client?.GetDatabase(_cosmosDatabaseContext.DatabaseId);
        var user = database?.GetUser(userId);
        var cosmosDbUserResponse = await user?.ReadAsync()!;

        return cosmosDbUserResponse.User;
    }
}