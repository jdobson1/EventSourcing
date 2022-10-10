using Microsoft.Azure.Cosmos;

namespace Core.Infrastructure;

public class CosmosDatabaseContext
{
    public CosmosDatabaseContext(string endpointUrl, string authorizationKey, string containerId, string databaseId, CosmosClientOptions options)
    {
        EndpointUrl = endpointUrl;
        AuthorizationKey = authorizationKey;
        ContainerId = containerId;
        DatabaseId = databaseId;
        Options = options;
    }
    public string EndpointUrl { get; set; }
    public string AuthorizationKey { get; set; }
    public string ContainerId { get; set; }
    public string DatabaseId { get; set; }
    public CosmosClientOptions Options { get; set; }
}