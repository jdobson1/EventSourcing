namespace Sagas;

public interface ISagaStateProvider
{
    Task DeleteAsync<TSagaState>(Guid id);
    Task<TSagaState> GetAsync<TSagaState>(Guid id) where TSagaState : class, ISagaState, new();
    Task<bool> SaveAsync(Guid id, ISagaState sagaState);
}