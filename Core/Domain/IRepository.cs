namespace Core.Domain
{
    public interface IRepository<T> where T : IAggregate
    {
        Task<T> GetById(Guid id, string clientId);
        Task Save(T aggregate, string clientId);
    }
}