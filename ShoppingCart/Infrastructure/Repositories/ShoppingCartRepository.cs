using Core.Domain;
using EventStore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCart.Infrastructure.Repositories
{
    public class ShoppingCartRepository : IRepository<Domain.ShoppingCart>
    {
        private readonly IEventStore _eventStore;

        public ShoppingCartRepository(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<Domain.ShoppingCart> GetById(Guid id, string clientId)
        {
            var streamId = $"shoppingcart:{id}";

            var stream = await _eventStore.LoadStreamAsync(clientId, streamId);

            return new Domain.ShoppingCart(stream.Events);
        }

        public async Task Save(Domain.ShoppingCart aggregate, string clientId)
        {
            if (aggregate.Events.Any())
            {
                var streamId = $"shoppingcart:{aggregate.Id}";

                await _eventStore.AppendToStreamAsync(
                      clientId,
                      streamId,
                      aggregate.Version,
                      aggregate.Events);
            }
        }
    }
}
