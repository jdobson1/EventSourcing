using Core.Domain;

namespace Subscriptions
{
    public interface IHandleEvents<T> where T : IEvent
    {
        Task Handle(T @event);
    }
}