namespace Core.Domain;

public interface IRepositorySnapShotDecorator<T> : IRepository<T> where T : IAggregate
{

}