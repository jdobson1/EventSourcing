using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace Products.Query.Functions
{
    public static class NegotiateFunction
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
                    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
                    [SignalRConnectionInfo(HubName = "productqueryviews")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }
    }
}
