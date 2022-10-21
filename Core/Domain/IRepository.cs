namespace Core.Domain
{
    public interface IRepository<T> where T : IAggregate
    {
        Task<T> GetById(Guid id, string clientId);
        Task<T> GetByIndexedProperty(string indexedPropertyValue);
        Task Save(T aggregate, string clientId);
    }
}