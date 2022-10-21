using Core.Domain;
using Inventory.Domain;
using Products.Common.Events;
using Subscriptions;
using System.Threading.Tasks;

namespace Inventory.EventHandlers
{
    public class ProductCreatedEventHandler : IHandleEvents<ProductCreated>
    {
        private readonly IRepository<ProductInventory> _repository;

        public ProductCreatedEventHandler(IRepository<ProductInventory> repository)
        {
            _repository = repository;
        }
        public async Task Handle(ProductCreated @event)
        {
            var productInventory = new ProductInventory(@event.ProductId, @event.UserId);
            await _repository.Save(productInventory, @event.UserId);
        }
    }
}
