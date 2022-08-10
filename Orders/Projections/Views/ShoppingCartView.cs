using ShoppingCart.Common.Dtos;
using System;
using System.Collections.Generic;

namespace Orders.Projections.Views
{
    public class ShoppingCartView
    {
        public Guid ShoppingCartId { get; set; }
        public List<ShoppingCartItemDto> Items { get; set; } = new List<ShoppingCartItemDto>();
    }
}
