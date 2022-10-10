using System;
using System.Collections.Generic;
using Core.Domain;
using Products.Common.Events;

namespace Products.Domain
{
    public class Product : AggregateBase
    {
        private string _name;

        public string Name
        {
            get => _name;
            set => Apply(new ProductNameChanged(Id, value, ClientId));
        }

        public string ClientId { get; private set; }

        #region Constructors
        public Product(Guid id, string name, string clientId) : base(id)
        {
            Apply(new ProductCreated(Id, name, clientId));
            ClientId = clientId;
        }

        internal Product(IEnumerable<IEvent> events)
        {
            Apply(events);
        }

        #endregion

        private void When(ProductCreated @event)
        {
            Id = @event.ProductId;
            _name = @event.ProductName;
            ClientId = @event.ClientId;
        }

        private void When(ProductNameChanged @event)
        {
            _name = @event.ProductName;
        }

        protected override void Mutate(IEvent @event)
        {
            ((dynamic)this).When((dynamic)@event);
        }
    }
}

