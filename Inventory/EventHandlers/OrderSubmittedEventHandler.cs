using Core.Domain;
using Inventory.Domain;
using Orders.Common.Events;
using Subscriptions;
using System.Threading.Tasks;

namespace Inventory.EventHandlers
{
    public class OrderSubmittedEventHandler : IHandleEvents<OrderSubmitted>
    {
        private readonly IRepository<ProductInventory> _repository;

        public OrderSubmittedEventHandler(IRepository<ProductInventory> repository)
        {
            _repository = repository;
        }

        public async Task Handle(OrderSubmitted @event)
        {
            foreach (var item in @event.Items)
            {
                var productInventory = await _repository.GetById(item.ProductId, @event.ClientId);
                productInventory.AdjustQuantityOnHand(item.Quantity);
                await _repository.Save(productInventory, @event.ClientId);
            }
        }
    }
}
