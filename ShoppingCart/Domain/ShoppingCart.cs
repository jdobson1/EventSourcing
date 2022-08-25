using Core.Domain;
using EventStore;
using ShoppingCart.Common.Events;
using System;
using System.Collections.Generic;

namespace ShoppingCart.Domain
{
    public class ShoppingCart : AggregateBase
    {
        private readonly List<ShoppingCartItem> _items = new();
        public IReadOnlyList<ShoppingCartItem> Items => _items;

        #region Constructors

        public ShoppingCart(Guid cartId) : base(cartId)
        {
            Apply(new ShoppingCartCreated(cartId));
        }

        internal ShoppingCart(IEnumerable<IEvent> events)
        {
            Apply(events);
        }

        #endregion

        public void AddItem(Guid productId, int quantity)
        {
            Apply(new ItemAddedToShoppingCart(Id, productId, quantity));
        }

        private void When(ShoppingCartCreated @event)
        {
            Id = @event.ShoppingCartId;
        }

        private void When(ItemAddedToShoppingCart @event)
        {
            _items.Add(new ShoppingCartItem(@event.ProductId, @event.Quantity));
        }

        protected override void Mutate(IEvent @event)
        {
            ((dynamic)this).When((dynamic)@event);
        }
    }
}
