using Core.Domain;

namespace Products.Common.Events
{
    public class ProductNameChanged : IEvent
    {
        public Guid ProductId {get; set;}
        public string ProductName {get; set;}
        public DateTime Timestamp {get; set;} = DateTime.UtcNow;
        public string ClientId { get; set; }

        public ProductNameChanged(Guid productId, string productName, string clientId)
        {
            ProductId = productId;
            ProductName = productName;
            ClientId = clientId;
        }

        public ProductNameChanged()
        {

        }
    }
}