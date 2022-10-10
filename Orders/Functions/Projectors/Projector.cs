using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using EventStore;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Projections;

namespace Orders.Functions.Projectors
{
    public class Projector
    {
        private readonly IProjectionEngine _projectionEngine;

        public Projector(IProjectionEngine projectionEngine)
        {
            _projectionEngine = projectionEngine;
        }

        [FunctionName("Projector")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "event-store",
            collectionName: "events",
            StartFromBeginning = true,
            ConnectionStringSetting = "cosmos-db-conn",
            LeaseCollectionPrefix = "order-projections",
            LeaseCollectionName = "leases")]IReadOnlyList<Document> changes,
            ILogger log)
        {
            if (changes == null || !changes.Any()) return;

            await _projectionEngine.HandleChangesAsync(changes.Select(c => JsonConvert.DeserializeObject<Change>(c.ToString())).ToList());
        }
    }
}
