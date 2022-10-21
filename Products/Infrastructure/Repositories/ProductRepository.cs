using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;
using EventStore;
using Microsoft.AspNetCore.Http;
using Products.Domain;

namespace Products.Infrastructure.Repositories
{
    public class ProductRepository : RepositoryBase<Product>, IRepository<Product>
    {
        private readonly IEventStore _eventStore;

        public ProductRepository(IEventStore eventStore, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _eventStore = eventStore;
        }

        public override async Task<Product> GetById(Guid id, string clientId)
        {
            var streamId = $"{clientId}:product:{id}";

            var stream = await _eventStore.LoadStreamAsync(clientId, streamId);

            return new Product(stream.Events);
        }

        public override async Task<Product> GetByIndexedProperty(string indexedPropertyValue)
        {
            var stream = await _eventStore.LoadStreamAsync(indexedPropertyValue);

            return new Product(stream.Events);
        }

        protected override async Task OnSave(Product aggregate, string clientId)
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