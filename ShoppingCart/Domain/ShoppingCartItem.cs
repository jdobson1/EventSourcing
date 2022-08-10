using System;

namespace ShoppingCart.Domain
{
    public class ShoppingCartItem
    {
        public ShoppingCartItem(Guid productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }

        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
