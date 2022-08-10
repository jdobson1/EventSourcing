using Core.Domain;
using EventStore;
using Inventory.Common.Events;
using System;
using System.Collections.Generic;

namespace Inventory.Domain
{
    public class ProductInventory : AggregateBase
    {
        public Guid ProductId { get; private set; }
        public int QuantityOnHand { get; private set; }

        public ProductInventory(Guid productId) : base(productId)
        {
            // apply event
            Apply(new ProductInventoryCreated(productId));
        }

        internal ProductInventory(IEnumerable<IEvent> events)
        {
            Apply(events);
        }

        public void AdjustQuantityOnHand(int adjustment)
        {
            Apply(new ProductInventoryQohAdjusted(Id, ProductId, adjustment));
        }

        protected override void Mutate(IEvent @event)
        {
            ((dynamic)this).When((dynamic)@event);
        }

        private void When(ProductInventoryCreated @event)
        {
            Id = @event.ProductId;
        }

        private void When(ProductInventoryQohAdjusted @event)
        {
            QuantityOnHand -= @event.Adjustment;
        }
    }
}
