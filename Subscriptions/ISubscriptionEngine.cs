using Core;
using EventStore;

namespace Subscriptions
{
    public interface ISubscriptionEngine
    {
        Task Initialize();
        void Subscribe(Subscription subscription);
        Task HandleChangesAsync(IReadOnlyCollection<Change> changes);
    }
}