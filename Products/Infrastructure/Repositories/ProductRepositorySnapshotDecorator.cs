using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain;
using EventStore;
using Microsoft.AspNetCore.Http;
using Products.Domain;

namespace Products.Infrastructure.Repositories
{
    public class ProductRepositorySnapshotDecorator : RepositoryBase<Product>, IRepository<Product>
    {
        private readonly ISnapshotStore _snapshotStore;
        private readonly IEventStore _eventStore;
        private readonly IRepository<Product> _innerProductRepository;

        public ProductRepositorySnapshotDecorator(IHttpContextAccessor httpContextAccessor, ISnapshotStore snapshotStore, IEventStore eventStore, IRepository<Product> innerProductRepository) : base(httpContextAccessor)
        {
            _snapshotStore = snapshotStore;
            _eventStore = eventStore;
            _innerProductRepository = innerProductRepository;
        }

        public override async Task<Product> GetById(Guid id, string clientId)
        {
            var streamId = $"{clientId}:product:{id}";

            var snapshot = await _snapshotStore.LoadSnapshotAsync(streamId);

            if (snapshot != null)
            {
                var streamTail = await _eventStore.LoadStreamAsync(clientId, streamId, snapshot.Version + 1);

                return new Product(snapshot.SnapshotData.ToObject<ProductSnapshot>(), snapshot.Version, streamTail.Events);
            }
            else
            {
                return await _innerProductRepository.GetById(id, clientId);
            }
        }

        public override async Task<Product> GetByIndexedProperty(string indexedPropertyValue, string clientId)
        {
            var stream = await _eventStore.LoadStreamAsync(indexedPropertyValue);
            var snapshot = await _snapshotStore.LoadSnapshotAsync(stream.Id);

            if (snapshot != null)
            {
                var streamTail = await _eventStore.LoadStreamAsync(clientId, stream.Id, snapshot.Version + 1);

                return new Product(snapshot.SnapshotData.ToObject<ProductSnapshot>(), snapshot.Version, streamTail.Events);
            }
            else
            {
                return new Product(stream.Events);
            }
        }

        public override Task<Product> GetByIndexedProperty(string indexedPropertyValue)
        {
            throw new NotImplementedException();
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