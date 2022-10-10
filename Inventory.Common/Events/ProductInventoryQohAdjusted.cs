using Core.Domain;

namespace Inventory.Common.Events
{
    public class ProductInventoryQohAdjusted : IEvent
    {
        public ProductInventoryQohAdjusted(Guid productInventoryId, Guid productId, int adjustment, string clientId)
        {
            ProductInventoryId = productInventoryId;
            ProductId = productId;
            Adjustment = adjustment;
            ClientId = clientId;
        }

        public ProductInventoryQohAdjusted()
        {
        }

        public DateTime Timestamp => DateTime.UtcNow;
        public string ClientId { get; set; }
        public Guid ProductInventoryId { get; set; }
        public Guid ProductId { get; set; }
        public int Adjustment { get; set; }
        public int NewQuantityOnHand { get; set; }
    }
}
