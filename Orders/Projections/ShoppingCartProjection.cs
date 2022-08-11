using Orders.Projections.Views;
using Projections;
using ShoppingCart.Common.Events;
using System.Linq;

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
            view.ShoppingCarts.Add(new Views.ShoppingCart(shoppingCartCreated.ShoppingCartId));
        }

        private void WhenItemAddedToCart(ItemAddedToShoppingCart itemAddedToShoppingCart, ShoppingCartView view)
        {
            var shoppingCart = view.ShoppingCarts.SingleOrDefault(x => x.ShoppingCartId == itemAddedToShoppingCart.ShoppingCartId);
            shoppingCart.Items.Add(new ShoppingCart.Common.Dtos.ShoppingCartItemDto
            {
                ProductId = itemAddedToShoppingCart.ProductId,
                Quantity = itemAddedToShoppingCart.Quantity
            });
        }
    }
}
