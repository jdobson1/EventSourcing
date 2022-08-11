using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;
using EventStore;
using Inventory.Domain;

namespace Inventory.Infrastructure.Repositories
{
    public class ProductInventoryRepository : IRepository<ProductInventory>
    {
        private readonly IEventStore _eventStore;

        public ProductInventoryRepository(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<ProductInventory> GetById(Guid id)
        {
            var streamId = $"productinv:{id}";

            var stream = await _eventStore.LoadStreamAsync(streamId);

            return new ProductInventory(stream.Events);
        }

        public async Task Save(ProductInventory aggregate)
        {
            if (aggregate.Events.Any())
            {
                var streamId = $"productinv:{aggregate.Id}";

              await _eventStore.AppendToStreamAsync(
                    streamId,
                    aggregate.Version,
                    aggregate.Events);
            }
        }
    }
}