using Core.Domain;
using Inventory.Common.Events;
using System;
using System.Collections.Generic;

namespace Inventory.Domain
{
    public class ProductInventory : AggregateBase
    {
        public Guid ProductId { get; private set; }
        public int QuantityOnHand { get; private set; }
        public string ClientId { get; private set; }

        #region Constructors

        public ProductInventory(Guid productId, string clientId) : base(productId)
        {
            Apply(new ProductInventoryCreated(productId, clientId));
        }

        internal ProductInventory(IEnumerable<IEvent> events)
        {
            Apply(events);
        }

        #endregion

        public void AdjustQuantityOnHand(int adjustment)
        {
            Apply(new ProductInventoryQohAdjusted(Id, ProductId, adjustment, ClientId));
        }

        private void When(ProductInventoryCreated @event)
        {
            Id = @event.ProductId;
            ClientId = @event.UserId;
        }

        private void When(ProductInventoryQohAdjusted @event)
        {
            QuantityOnHand -= @event.Adjustment;
        }

        protected override void Mutate(IEvent @event)
        {
            ((dynamic)this).When((dynamic)@event);
        }
    }
}
