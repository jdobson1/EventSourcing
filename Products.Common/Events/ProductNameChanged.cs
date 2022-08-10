using EventStore;
using System;

namespace Products.Common.Events
{
    public class ProductNameChanged : IEvent
    {
        public Guid ProductId {get; set;}
        public string ProductName {get; set;}
        public DateTime Timestamp {get; set;} = DateTime.UtcNow;

        public ProductNameChanged(Guid productId, string productName)
        {
            ProductId = productId;
            ProductName = productName;
        }

        public ProductNameChanged()
        {

        }
    }
}