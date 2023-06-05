using Core.Domain;
using EventStore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace ShoppingCart.Infrastructure.Repositories
{
    public class ShoppingCartRepository : IRepository<Domain.ShoppingCart>
    {
        private readonly IEventStore _eventStore;
        private readonly ILogger<ShoppingCartRepository> _logger;

        public ShoppingCartRepository(IEventStore eventStore, ILogger<ShoppingCartRepository> logger)
        {
            _eventStore = eventStore;
            _logger = logger;
        }

        public async Task<Domain.ShoppingCart> GetById(Guid id, string clientId)
        {
            try
            {
                var streamId = $"{clientId}:shoppingcart:{id}";

                var stream = await _eventStore.LoadStreamAsync(clientId, streamId);

                return new Domain.ShoppingCart(stream.Events);
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex.Message);
            }

            return new Domain.ShoppingCart(id, clientId);
        }

        public Task<Domain.ShoppingCart> GetByIndexedProperty(string indexedPropertyValue, string clientId)
        {
            throw new NotImplementedException();
        }

        public Task<Domain.ShoppingCart> GetByIndexedProperty(string indexedPropertyValue)
        {
            throw new NotImplementedException();
        }

        public async Task Save(Domain.ShoppingCart aggregate, string clientId)
        {
            if (aggregate.Events.Any())
            {
                var streamId = $"{clientId}:shoppingcart:{aggregate.Id}";

                await _eventStore.AppendToStreamAsync(
                      clientId,
                      streamId,
                      aggregate.Version,
                      aggregate.Events);
            }
        }
    }
}
