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
        public string ClientId { get; private set; }
        #region Constructors

        public ShoppingCart(Guid cartId, string clientId) : base(cartId)
        {
            Apply(new ShoppingCartCreated(cartId, clientId));
        }

        internal ShoppingCart(IEnumerable<IEvent> events)
        {
            Apply(events);
        }

        #endregion

        public void AddItem(Guid productId, int quantity)
        {
            Apply(new ItemAddedToShoppingCart(Id, productId, quantity, ClientId));
        }

        private void When(ShoppingCartCreated @event)
        {
            Id = @event.ShoppingCartId;
            ClientId = @event.ClientId;
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
