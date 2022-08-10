using Orders.Common.Dtos;

namespace Orders.Common.Commands
{
    public class SubmitOrder
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public AddressDto BillingAddress { get; set; }
        public AddressDto ShippingAddress { get; set; }
        public Guid ShoppingCartId { get; set; }
    }
}
