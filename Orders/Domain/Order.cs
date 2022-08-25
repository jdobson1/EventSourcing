using Orders.Common.Events;
using Core.Domain;
using EventStore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orders.Domain
{
    public class Order : AggregateBase
    {
        public Guid ShoppingCartId { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public Address BillingAddress { get; private set; }
        public Address ShippingAddress { get; private set; }
        public List<OrderItem> Items { get; private set; }

        #region Constructors

        public Order(Guid shoppingCartId, string firstName, string lastName, Address billingAddress, Address shippingAddress, List<OrderItem> items) : base(Guid.NewGuid())
        {
            if (shoppingCartId == Guid.Empty)
                throw new DomainException("ShoppingCartId is required");

            if (billingAddress == null)
                throw new DomainException("Billing address is required");

            if (shippingAddress == null)
                throw new DomainException("Shipping address is required");

            if (!items.Any())
                throw new DomainException("Order items are required");

            Apply(new OrderSubmitted(Id, shoppingCartId, firstName, lastName, billingAddress.ToDto(), shippingAddress.ToDto(), items.Select(i => i.ToDto()).ToList()));
        }

        internal Order(IEnumerable<IEvent> events)
        {
            Apply(events);
        }

        #endregion

        private void When(OrderSubmitted @event)
        {
            Id = @event.OrderId;
            ShoppingCartId = @event.ShoppingCartId;
            BillingAddress = new Address(@event.BillingAddress);
            ShippingAddress = new Address(@event.ShippingAddress);
            Items = @event.Items.Select(i => new OrderItem(i)).ToList();
            FirstName = @event.FirstName;
            LastName = @event.LastName;
        }

        protected override void Mutate(IEvent @event)
        {
            ((dynamic)this).When((dynamic)@event);
        }
    }
}