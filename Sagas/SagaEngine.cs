using System.Linq.Expressions;
using System.Reflection;
using Core.Domain;
using EventStore;
using Newtonsoft.Json.Serialization;

namespace Sagas;

public class SagaEngine : ISagaEngine
{
    private readonly IEventTypeResolver _eventTypeResolver;
    private readonly List<Saga> _sagas = new();

    public SagaEngine(IEventTypeResolver eventTypeResolver)
    {
        _eventTypeResolver = eventTypeResolver;
    }


    public void RegisterSaga(Saga saga)
    {
        saga.Initialize();
        _sagas.Add(saga);
    }
    
    public async Task HandleEventAsync(IReadOnlyCollection<Change> changes)
    {
        foreach (var change in changes)
        {
            var @event = change.GetEvent(_eventTypeResolver);

            if (@event == null) continue;

            var subscribedSagas = _sagas
                .Where(saga => saga.IsSubscribedTo(@event));

            foreach (var saga in subscribedSagas)
            {
                var handled = false;

                while (!handled)
                {
                    handled = await saga.HandleAsync(@event);
                        
                    if (!handled)
                        await Task.Delay(100);
                }
            }
        }
    }
}

