using Products.Common.Events;
using Products.Query.Projections.Views;
using Projections;
using System.Linq;

namespace Products.Query.Projections
{
    public class ProductsProjection : Projection<ProductsView>
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
    }
}
