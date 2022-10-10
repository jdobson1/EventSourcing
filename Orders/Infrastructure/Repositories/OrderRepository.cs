using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;
using EventStore;
using Orders.Domain;

namespace Orders.Infrastructure.Repositories
{
    public class OrderRepository : IRepository<Order>
    {
        private readonly IEventStore _eventStore;

        public OrderRepository(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<Order> GetById(Guid id, string clientId)
        {
            var streamId = $"order:{id}";

            var stream = await _eventStore.LoadStreamAsync(clientId, streamId);

            return new Order(stream.Events);
        }

        public async Task Save(Order aggregate, string clientId)
        {
            if (aggregate.Events.Any())
            {
                var streamId = $"order:{aggregate.Id}";

              await _eventStore.AppendToStreamAsync(
                    clientId,
                    streamId,
                    aggregate.Version,
                    aggregate.Events);
            }
        }
    }
}