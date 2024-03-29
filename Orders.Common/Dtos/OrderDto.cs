﻿namespace Orders.Common.Dtos
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public AddressDto BillingAddress { get; set; }
        public AddressDto ShippingAddress { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}
