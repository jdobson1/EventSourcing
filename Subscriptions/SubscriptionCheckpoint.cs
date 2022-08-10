using System.Collections.Generic;
using Core;
using Newtonsoft.Json;

namespace Projections
{
    public class SubscriptionCheckpoint
    {
        public SubscriptionCheckpoint()
        {
            LogicalSequenceNumber = -1;
            ItemIds = new List<string>();
        }
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("lsn")]
        public long LogicalSequenceNumber { get; set; }

        [JsonProperty("itemIds")]
        public List<string> ItemIds { get; }

        public bool IsNewerThanCheckpoint(Change change)
        {
            if (change.LogicalSequenceNumber == LogicalSequenceNumber)
            {
                return !ItemIds.Contains(change.Id);
            }

            return change.LogicalSequenceNumber > LogicalSequenceNumber;
        }

        public void UpdateCheckpoint(Change change)
        {
            if (change.LogicalSequenceNumber != LogicalSequenceNumber)
            {
                LogicalSequenceNumber = change.LogicalSequenceNumber;
                ItemIds.Clear();
            }

            ItemIds.Add(change.Id);
        }
    }
}