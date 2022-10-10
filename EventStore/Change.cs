using Newtonsoft.Json;

namespace EventStore
{
    public class Change : EventWrapper
    {
        [JsonProperty("_lsn")]
        public long LogicalSequenceNumber { get; set; }
    }
}