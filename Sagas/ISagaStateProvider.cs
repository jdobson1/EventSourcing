namespace Sagas;

public interface ISagaStateProvider
{
    Task<bool> DeleteAsync(Guid correlationId);
    Task<SagaState> GetAsync(Guid correlationId);
    Task<bool> IsCompletedAsync(Guid correlationId);
    Task<bool> SaveAsync(Guid correlationId, SagaState sagaState);
}