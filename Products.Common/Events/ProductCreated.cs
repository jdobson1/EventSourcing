using Core.Domain;

namespace Products.Common.Events
{
    public class ProductCreated : IEvent
    {
        public Guid ProductId {get; set;}
        [EventStore.Index("productName")]
        public string ProductName {get; set;}
        public Guid ProductCategoryId {get; set;}
        public DateTime Timestamp {get; set;} = DateTime.UtcNow;
        public string UserId { get; set; }

        public ProductCreated(Guid productId, string productName, string clientId)
        {
            ProductId = productId;
            ProductName = productName;
            UserId = clientId;
        }

        public ProductCreated()
        {

        }
    }
}