using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using EventStore;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Subscriptions;

namespace Inventory.Functions
{
    public class Subscriber
    {
        private readonly ISubscriptionEngine _subscriptionEngine;

        public Subscriber(ISubscriptionEngine subscriptionEngine)
        {
            _subscriptionEngine = subscriptionEngine;
        }

        [FunctionName("Subscriber")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "event-store",
            collectionName: "events",
            StartFromBeginning = true,
            ConnectionStringSetting = "cosmos-db-conn",
            LeaseCollectionPrefix = "inventory-subscriptions",
            LeaseCollectionName = "leases")]IReadOnlyList<Document> changes,
            ILogger log)
        {
            if (changes == null || !changes.Any()) return;

            await _subscriptionEngine.HandleChangesAsync(changes.Select(c => JsonConvert.DeserializeObject<Change>(c.ToString())).ToList());
        }
    }
}
