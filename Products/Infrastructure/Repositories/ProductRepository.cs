using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;
using EventStore;
using Products.Domain;

namespace Products.Infrastructure.Repositories
{
    public class ProductRepository : IRepository<Product>
    {
        private readonly IEventStore _eventStore;

        public ProductRepository(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<Product> GetById(Guid id, string clientId)
        {
            var streamId = $"{clientId}:product:{id}";

            var stream = await _eventStore.LoadStreamAsync(clientId, streamId);

            return new Product(stream.Events);
        }

        public async Task Save(Product aggregate, string clientId)
        {
            if (aggregate.Events.Any())
            {
                var streamId = $"{clientId}:product:{aggregate.Id}";

              await _eventStore.AppendToStreamAsync(
                    clientId,
                    streamId,
                    aggregate.Version,
                    aggregate.Events);
            }
        }
    }
}