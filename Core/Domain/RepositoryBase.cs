using Microsoft.AspNetCore.Http;

namespace Core.Domain
{
    public abstract class RepositoryBase<T> : IRepository<T> where T : IAggregate
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        protected RepositoryBase(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected abstract Task OnSave(T aggregate, string clientId);

        public abstract Task<T> GetById(Guid id, string clientId);

        public abstract Task<T> GetByIndexedProperty(string indexedPropertyValue);

        public virtual async Task Save(T aggregate, string clientId)
        {
            foreach (var @event in aggregate.Events)
                @event.UserId = _httpContextAccessor.HttpContext.User.Identity?.Name ?? string.Empty;

            await OnSave(aggregate, clientId);
        }
    }
}
