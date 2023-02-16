using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Projections;

namespace Products.Query.Functions.Projectors
{
    public class Projector
    {
        private readonly ITenantProjectionEngine _projectionEngine;
        private IAsyncCollector<SignalRMessage> _signalRMessages;

        public Projector(ITenantProjectionEngine projectionEngine)
        {
            _projectionEngine = projectionEngine;
        }

        [FunctionName("Projector")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "event-store",
            collectionName: "events",
            StartFromBeginning = true,
            FeedPollDelay = 500,
            ConnectionStringSetting = "cosmos-db-conn",
            LeaseCollectionPrefix = "prod-projections",
            LeaseCollectionName = "leases")]IReadOnlyList<Document> changes,
            ILogger log,
            [SignalR(HubName = "productqueryviews")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            if (changes == null || !changes.Any()) return;

            _signalRMessages = signalRMessages;
            _projectionEngine.OnChangesHandled += OnChangesHandled;

            await _projectionEngine.HandleChangesAsync(changes.Select(c => JsonConvert.DeserializeObject<Change>(c.ToString())).ToList());
        }

        private void OnChangesHandled(object sender, ChangesHandledEventArgs e)
        {
            _signalRMessages.AddAsync(new SignalRMessage { Target = e.View.Name, Arguments = new object[] { e.View.Payload }});
        }
    }
}
