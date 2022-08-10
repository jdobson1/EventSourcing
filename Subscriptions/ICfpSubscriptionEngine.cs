namespace Subscriptions
{
    public interface ICfpSubscriptionEngine
    {
        void Subscribe(Subscription subscription);

        Task StartAsync();

        Task StopAsync();
    }
}