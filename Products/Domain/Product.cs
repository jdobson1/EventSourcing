using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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
            if (string.IsNullOrEmpty(name))
                throw new DomainException("Name is required");

            Apply(new ProductCreated(Id, name, clientId));
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
            ClientId = @event.UserId;
        }

        private void When(ProductNameChanged @event)
        {
            _name = @event.ProductName;
        }

        protected override void Mutate(IEvent @event)
        {
            ((dynamic)this).When((dynamic)@event);
        }

        #region Snapshot Functionality

        public Product(ProductSnapshot snapshot, int version, IEnumerable<IEvent> events)
        {
            Id = snapshot.Id;
            Name = snapshot.Name;
            ClientId = snapshot.ClientId;
            Version = version;

            Apply(events);
        }

        public ProductSnapshot GetSnapshot()
        {
            return new ProductSnapshot(Id, Name, ClientId);
        }

        #endregion
    }
}

