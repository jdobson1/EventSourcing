using EventStore;

namespace ShoppingCart.Common.Events
{
    public class ItemAddedToShoppingCart : IEvent
    {
        public ItemAddedToShoppingCart(Guid id, Guid productId, int quantity)
        {
            ShoppingCartId = id;
            ProductId = productId;
            Quantity = quantity;
        }

        public ItemAddedToShoppingCart()
        {
        }

        public DateTime Timestamp => DateTime.UtcNow;
        public Guid ShoppingCartId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
