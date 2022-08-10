using EventStore;
using Orders.Common.Dtos;

namespace Orders.Common.Events
{
    public class OrderSubmitted : IEvent
    {
        public Guid OrderId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public AddressDto BillingAddress { get; set; }
        public AddressDto ShippingAddress { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public Guid ShoppingCartId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public OrderSubmitted()
        {

        }

        public OrderSubmitted(Guid orderId, Guid shoppingCartId, string firstName, string lastName, AddressDto billingAddress, AddressDto shippingAddress, List<OrderItemDto> items)
        {
            OrderId = orderId;
            ShoppingCartId = shoppingCartId;
            FirstName = firstName;
            LastName = lastName;
            BillingAddress = billingAddress;
            ShippingAddress = shippingAddress;
            Items = items;
        }
    }
}
