using Core.Domain;

namespace Products.Common.Events
{
    public class ProductNameChanged : IEvent
    {
        public Guid ProductId {get; set;}
        public string ProductName {get; set;}
        public DateTime Timestamp {get; set;} = DateTime.UtcNow;
        public string UserId { get; set; }

        public ProductNameChanged(Guid productId, string productName, string clientId)
        {
            ProductId = productId;
            ProductName = productName;
            UserId = clientId;
        }

        public ProductNameChanged()
        {

        }
    }
}