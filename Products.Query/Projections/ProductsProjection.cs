using System;
using Products.Common.Events;
using Products.Query.Projections.Views;
using Projections;
using System.Linq;

namespace Products.Query.Projections
{
    public class ProductsProjection : TenantProjection<ProductsView>
    {
        public ProductsProjection()
        {
            RegisterHandler<ProductCreated>(WhenCreated);
            RegisterHandler<ProductNameChanged>(WhenProductNameChanged);
        }

        private void WhenCreated(ProductCreated productCreated, ProductsView view)
        {
            view.Products.Add(new Common.Dtos.ProductDto { Id = productCreated.ProductId, Name = productCreated.ProductName });
        }

        private void WhenProductNameChanged(ProductNameChanged productNameChanged, ProductsView view)
        {
            var product = view.Products.SingleOrDefault(x => x.Id == productNameChanged.ProductId);
            product.Name = productNameChanged.ProductName;
        }

        public override Guid GetClientId(string streamId)
        {
            // parse streamId to get the client id
            var streamParts = streamId.Split(":");
            return Guid.Parse(streamParts[0]);
        }
    }
}
