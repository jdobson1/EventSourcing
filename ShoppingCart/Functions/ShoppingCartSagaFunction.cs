using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sagas;

namespace ShoppingCart.Functions
{
    public class ShoppingCartSagaFunction
    {
        private readonly ISagaEngine _sagaEngine;

        public ShoppingCartSagaFunction(ISagaEngine sagaEngine)
        {
            _sagaEngine = sagaEngine;
        }

        [FunctionName("ShoppingCartSagaFunction")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "sagas",
            collectionName: "state",
            StartFromBeginning = true,
            FeedPollDelay = 500,
            ConnectionStringSetting = "cosmos-db-conn",
            LeaseCollectionName = "leases")]IReadOnlyList<Document> changes,
            ILogger log)
        {
            if (changes == null || !changes.Any()) return;

            await _sagaEngine.HandleEventAsync(changes.Select(c => JsonConvert.DeserializeObject<PlatformMessage>(c.ToString()))
                .ToList());
        }
    }
}
