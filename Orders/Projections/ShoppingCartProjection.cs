using Orders.Projections.Views;
using Projections;
using ShoppingCart.Common.Events;

namespace Orders.Projections
{
    public class ShoppingCartProjection : Projection<ShoppingCartView>
    {
        public ShoppingCartProjection()
        {
            RegisterHandler<ShoppingCartCreated>(WhenCreated);
            RegisterHandler<ItemAddedToShoppingCart>(WhenItemAddedToCart);
        }

        private void WhenCreated(ShoppingCartCreated shoppingCartCreated, ShoppingCartView view)
        {
            view.ShoppingCartId = shoppingCartCreated.ShoppingCartId;
        }

        private void WhenItemAddedToCart(ItemAddedToShoppingCart itemAddedToShoppingCart, ShoppingCartView view)
        {
            view.Items.Add(new ShoppingCart.Common.Dtos.ShoppingCartItemDto
            {
                ProductId = itemAddedToShoppingCart.ProductId,
                Quantity = itemAddedToShoppingCart.Quantity
            });
        }
    }
}
