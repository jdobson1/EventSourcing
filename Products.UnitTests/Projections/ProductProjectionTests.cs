using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Products.Common.Events;
using Products.Query.Projections;
using Products.Query.Projections.Views;

namespace Products.UnitTests.Projections
{
    [TestClass]
    public class ProductProjectionTests
    {
        [TestMethod]
        public void ShouldAddProductToView()
        {
            var productId = Guid.NewGuid();
            var productName = "product-name";
            var clientId = Guid.NewGuid();
            var productsView = new ProductsView() { Payload = new JObject() };
            var projection = new ProductsProjection();
            var createdEvent = new ProductCreated(productId, productName, clientId.ToString());
            
            projection.Apply(createdEvent, productsView);
            
            var updatedProductsView = JsonConvert.DeserializeObject<ProductsView>(productsView.Payload.ToString());
            var product = updatedProductsView.Products.FirstOrDefault();
            
            Assert.IsTrue(product?.Id == productId);
            Assert.IsTrue(product?.Name == productName);
        }
    }
}
