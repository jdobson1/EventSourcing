using EventStore;

namespace Inventory.Common.Events
{
    public class ProductInventoryQohAdjusted : IEvent
    {
        public ProductInventoryQohAdjusted(Guid productInventoryId, Guid productId, int adjustment)
        {
            ProductInventoryId = productInventoryId;
            ProductId = productId;
            Adjustment = adjustment;
        }

        public ProductInventoryQohAdjusted()
        {
        }

        public DateTime Timestamp => DateTime.UtcNow;
        public Guid ProductInventoryId { get; set; }
        public Guid ProductId { get; set; }
        public int Adjustment { get; set; }
        public int NewQuantityOnHand { get; set; }
    }
}
