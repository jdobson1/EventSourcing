using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using Core.Infrastructure;
using Microsoft.Azure.Cosmos;
using CosmosDbResourceTokenProvider.Common.Queries;

namespace CosmosDbResourceTokenProvider
{
    public class RetrieveTokenFunction
    {
        private readonly ICosmosClientFactory _cosmosClientFactory;
        private CosmosClient _client;

        public RetrieveTokenFunction(ICosmosClientFactory cosmosClientFactory)
        {
            _cosmosClientFactory = cosmosClientFactory;
        }

        [FunctionName("RetrieveToken")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Retrieving token...");
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            RetrieveToken retrieveToken = JsonConvert.DeserializeObject<RetrieveToken>(requestBody);

            BuildClient();
            
            var user = await CreateUserAsync(retrieveToken.UserId, retrieveToken.DatabaseId);

            var permission = await CreatePermissionAsync(user, retrieveToken.ResourceUri, retrieveToken.PartitionKey,
                retrieveToken.DatabaseId, retrieveToken.ContainerId);

            var resourceToken = permission.ReadAsync().Result.Resource.Token;

            return new OkObjectResult(resourceToken);
        }

        private void BuildClient()
        {
            _client = _cosmosClientFactory.Create(Startup.EndpointUrl, Startup.AuthorizationKey,
                new CosmosClientOptions() { ConnectionMode = ConnectionMode.Gateway });
        }

        private async Task<User> CreateUserAsync(string userId, string databaseId)
        {
            User cosmosDbUser = null;
            var database = _client.GetDatabase(databaseId);
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

        private async Task<Permission> CreatePermissionAsync(User user, string resourceUri, string partitionKey, string databaseId, string containerId)
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
                    var container = _client.GetContainer(databaseId, containerId);

                    await user?.CreatePermissionAsync(
                        new PermissionProperties(
                            id: permissionId,
                            permissionMode: PermissionMode.All,
                            container: container,
                            resourcePartitionKey: new PartitionKey(partitionKey)))!;
                }
            }

            return permission;
        }
    }
}
