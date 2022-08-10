using EventStore;
using System;

namespace Products.Common.Events
{
    public class ProductCreated : IEvent
    {
        public Guid ProductId {get; set;}
        public string ProductName {get; set;}
        public Guid ProductCategoryId {get; set;}
        public DateTime Timestamp {get; set;} = DateTime.UtcNow;

        public ProductCreated(Guid productId, string productName)
        {
            ProductId = productId;
            ProductName = productName;
        }

        public ProductCreated()
        {

        }
    }
}