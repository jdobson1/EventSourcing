using Microsoft.Azure.Cosmos;

namespace Core.Infrastructure;

public class CosmosDatabaseContext
{
    public CosmosDatabaseContext(string endpointUrl, string authorizationKey, string containerId, string databaseId, int resourceTokenExpirationSecs, CosmosClientOptions options)
    {
        EndpointUrl = !string.IsNullOrEmpty(endpointUrl) ? endpointUrl : throw new ArgumentNullException(nameof(endpointUrl));
        AuthorizationKey = !string.IsNullOrEmpty(authorizationKey) ? authorizationKey : throw new ArgumentNullException(nameof(authorizationKey));
        ContainerId = !string.IsNullOrEmpty(containerId) ? containerId : throw new ArgumentNullException(nameof(containerId));
        DatabaseId = !string.IsNullOrEmpty(databaseId) ? databaseId : throw new ArgumentNullException(nameof(databaseId));
        Options = options;
        ResourceTokenExpirationSecs = resourceTokenExpirationSecs == 0 ? 18000 : resourceTokenExpirationSecs;
    }
    public string EndpointUrl { get; set; }
    public string AuthorizationKey { get; set; }
    public string ContainerId { get; set; }
    public string DatabaseId { get; set; }
    public int ResourceTokenExpirationSecs { get; set; }
    public CosmosClientOptions Options { get; set; }
}