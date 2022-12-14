using Core.Domain;
using Products.Domain;

namespace Products.UnitTests.Domain
{
    [TestClass]
    public class ProductAggregateTests
    {
        private readonly FakeRepository<Product> _fakeRepository = new();

        [TestMethod]
        public void ShouldCreateProductAndApplyEvents()
        {
            var productId = Guid.NewGuid();
            var productName = "Test Product";
            var clientId = Guid.NewGuid().ToString();

            var product = new Product(productId, productName, clientId);

            _fakeRepository.Save(product, clientId);

            Assert.AreEqual(productId, product.Id);
            Assert.AreEqual(productName, product.Name);
            Assert.AreEqual(clientId, product.ClientId);
        }

        [TestMethod]
        public async Task ShouldCreateProductAndChangeName()
        {
            var productId = Guid.NewGuid();
            var productName = "Test Product";
            var newProductName = "New Test Product Name";
            var clientId = Guid.NewGuid().ToString();

            var product = new Product(productId, productName, clientId);

            await _fakeRepository.Save(product, clientId);

            Assert.IsTrue(product.Events.Count == 1);
            Assert.AreEqual(productId, product.Id);
            Assert.AreEqual(productName, product.Name);

            var productCreated = await _fakeRepository.GetById(productId, clientId);

            productCreated.Name = newProductName;

            await _fakeRepository.Save(productCreated, string.Empty);

            Assert.IsTrue(productCreated.Events.Count == 1);
            Assert.AreEqual(productCreated.Name, newProductName);

            var productNameChanged = await _fakeRepository.GetById(productId, clientId);

            Assert.AreEqual(newProductName, productNameChanged.Name);
        }

        [TestMethod]
        public async Task ShouldGetProductByIndex()
        {
            var productId = Guid.NewGuid();
            var productName = "Test Product";
            var clientId = Guid.NewGuid().ToString();

            var product = new Product(productId, productName, clientId);

            await _fakeRepository.Save(product, clientId);

            var savedProduct = await _fakeRepository.GetByIndexedProperty(productName);

            Assert.IsNotNull(savedProduct);
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