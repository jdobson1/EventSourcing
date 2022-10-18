using Core.Domain;
using Products.Domain;

namespace Products.UnitTests
{
    [TestClass]
    public class ProductAggregateTests
    {
        [TestMethod]
        public void ShouldCreateProductAndApplyEvents()
        {
            var productId = Guid.NewGuid();
            var productName = "Test Product";
            var clientId = Guid.NewGuid().ToString();

            var product = new Product(productId, productName, clientId);

            Assert.IsTrue(product.Events.Any());
            product.Apply(product.Events);
            Assert.AreEqual(productId, product.Id);
            Assert.AreEqual(productName, product.Name);
            Assert.AreEqual(clientId, product.ClientId);
        }

        [TestMethod]
        public void ShouldThrowNameRequiredExceptionWhenCreatingProduct()
        {
            var productId = Guid.NewGuid();
            var clientId = Guid.NewGuid().ToString();

            Assert.ThrowsException<DomainException>(() => new Product(productId, null, clientId), "Name is required");
        }
    }
}