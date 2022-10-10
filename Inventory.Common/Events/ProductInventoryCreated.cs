using Core.Domain;

namespace Inventory.Common.Events
{
    public class ProductInventoryCreated : IEvent
    {
        public ProductInventoryCreated(Guid productId, string clientId)
        {
            ProductId = productId;
            ClientId = clientId;
        }

        public DateTime Timestamp => DateTime.UtcNow;
        public string ClientId { get; set; }

        public Guid ProductId { get; set; }
    }
}
