using Core.Domain;

namespace Sagas
{
    public interface ISaga
    {   
        Task<bool> HandleAsync(IEvent @event);
        Task CompleteAsync();
        bool IsActive();
        bool IsCompleted();
        bool IsSubscribedTo(IEvent @event);
        
    }
}
