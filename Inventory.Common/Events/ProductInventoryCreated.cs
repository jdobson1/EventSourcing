using EventStore;

namespace Inventory.Common.Events
{
    public class ProductInventoryCreated : IEvent
    {
        public ProductInventoryCreated(Guid productId)
        {
            ProductId = productId;
        }

        public DateTime Timestamp => DateTime.UtcNow;

        public Guid ProductId { get; set; }
    }
}
