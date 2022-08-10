using EventStore;
using Newtonsoft.Json;

namespace Core
{
    public class Change : EventWrapper
    {
        [JsonProperty("_lsn")]
        public long LogicalSequenceNumber { get; set; }
    }
}