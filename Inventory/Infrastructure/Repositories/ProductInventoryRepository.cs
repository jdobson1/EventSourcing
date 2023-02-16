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

        public async Task<ProductInventory> GetById(Guid id, string clientId)
        {
            var streamId = $"{clientId}:productinv:{id}";

            var stream = await _eventStore.LoadStreamAsync(clientId, streamId);

            return new ProductInventory(stream.Events);
        }

        public Task<ProductInventory> GetByIndexedProperty(string indexedPropertyValue, string clientId)
        {
            throw new NotImplementedException();
        }

        public Task<ProductInventory> GetByIndexedProperty(string indexedPropertyValue)
        {
            throw new NotImplementedException();
        }

        public async Task Save(ProductInventory aggregate, string clientId)
        {
            if (aggregate.Events.Any())
            {
                var streamId = $"productinv:{aggregate.Id}";

              await _eventStore.AppendToStreamAsync(
                    clientId,
                    streamId,
                    aggregate.Version,
                    aggregate.Events);
            }
        }
    }
}