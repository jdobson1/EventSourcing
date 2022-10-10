namespace CosmosDbResourceTokenProvider.Common.Queries
{
    public class RetrieveToken
    {
        public string UserId { get; set; }
        public string DatabaseId { get; set; }
        public string ResourceUri { get; set; }
        public string PartitionKey { get; set; }
        public string ContainerId { get; set; }
    }
}