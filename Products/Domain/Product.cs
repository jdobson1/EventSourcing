using System;
using System.Collections.Generic;
using EventStore;
using Core.Domain;
using Products.Common.Events;

namespace Products.Domain
{
    public class Product : AggregateBase
    {
        private string _name;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                Apply(new ProductNameChanged(Id, value));
            }
        }

        #region Constructors
        public Product(Guid id, string name) : base(id)
        {
            Apply(new ProductCreated(Id, name));
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

