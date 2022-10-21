using Core.Domain;

namespace ShoppingCart.Common.Events
{
    public class ShoppingCartCreated : IEvent
    {
        public DateTime Timestamp => DateTime.UtcNow;
        public string UserId { get; set; }
        public Guid ShoppingCartId { get; set; }

        public ShoppingCartCreated(Guid shoppingCartId, string clientId)
        {
            ShoppingCartId = shoppingCartId;
            UserId = clientId;
        }
    }
}
