using EventStore;

namespace ShoppingCart.Common.Events
{
    public class ShoppingCartCreated : IEvent
    {
        public DateTime Timestamp => DateTime.UtcNow;
        public Guid ShoppingCartId { get; set; }

        public ShoppingCartCreated(Guid shoppingCartId)
        {
            ShoppingCartId = shoppingCartId;
        }
    }
}
