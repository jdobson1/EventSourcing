using EventStore;
using Newtonsoft.Json;

namespace Subscriptions
{
    public class Subscription
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("eventHandlerType")]
        public string EventHandlerType { get; set; }
    }
}