using System;
using Orders.Common.Dtos;

namespace Orders.Domain
{
    public class OrderItem
    {
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }

        public OrderItem(Guid productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }

        public OrderItemDto ToDto()
        {
            return new OrderItemDto
            {
                ProductId = ProductId,
                Quantity = Quantity
            };
        }

        public OrderItem(OrderItemDto dto)
        {
            ProductId = dto.ProductId;
            Quantity = dto.Quantity;
        }
    }
}