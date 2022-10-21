using Core.Domain;

namespace ShoppingCart.Common.Events
{
    public class ItemAddedToShoppingCart : IEvent
    {
        public ItemAddedToShoppingCart(Guid id, Guid productId, int quantity, string clientId)
        {
            ShoppingCartId = id;
            ProductId = productId;
            Quantity = quantity;
            UserId = clientId;
        }

        public ItemAddedToShoppingCart()
        {
        }

        public DateTime Timestamp => DateTime.UtcNow;
        public string UserId { get; set; }
        public Guid ShoppingCartId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
