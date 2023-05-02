using Core.Domain;

namespace Sagas;

public class SagaEngine : ISagaEngine
{
    private readonly IEventTypeResolver _eventTypeResolver;
    private readonly List<ISaga> _sagas = new();

    public SagaEngine(IEventTypeResolver eventTypeResolver)
    {
        _eventTypeResolver = eventTypeResolver;
    }


    public void RegisterSaga(ISaga saga)
    {
        _sagas.Add(saga);
    }

    public async Task HandleEventAsync(IReadOnlyCollection<PlatformMessage> platformMessages)
    {
        foreach (var platformMessage in platformMessages)
        {
            var @event = platformMessage.GetEvent(_eventTypeResolver);

            var subscribedSagas = _sagas
                .Where(saga => saga.IsSubscribedTo(@event));

            foreach (var saga in subscribedSagas)
            {
                var handled = false;

                while (!handled)
                {
                    handled = await saga.HandleAsync(@event);
                        
                    if (saga.IsCompleted()) continue;
                        
                    if (!handled)
                        await Task.Delay(100);
                }
            }
        }
    }
}