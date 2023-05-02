using Core.Domain;
using Newtonsoft.Json.Linq;

namespace Sagas;

public abstract class Saga<TStartedBy, TState> : ISaga where TState : SagaState where TStartedBy : IEvent
{
    public TState State;
    public readonly ISagaStateProvider StateProvider;
    private readonly Dictionary<Type, Action<IEvent>?> _handlers;
    private readonly bool _deleteUponCompletion;

    protected Saga(ISagaStateProvider stateProvider, bool deleteUponCompletion = false)
    {
        StateProvider = stateProvider;
        _deleteUponCompletion = deleteUponCompletion;
        _handlers = new Dictionary<Type, Action<IEvent>?>();
    }

    public async Task<bool> HandleAsync(IEvent @event)
    {
        var sagaCorrelation = OnCorrelating(@event);

        if (!sagaCorrelation.IsCorrelated) return false;
        
        State = await LoadStateAsync(sagaCorrelation.CorrelationId.GetValueOrDefault());

        if (State.Status == SagaStatus.Completed) return false;

        if (@event.GetType() == typeof(TStartedBy) && State.CorrelationId == Guid.Empty)
            Start(sagaCorrelation.CorrelationId.GetValueOrDefault());

        return await ApplyAsync(@event);
    }
    
    public abstract SagaCorrelation OnCorrelating(IEvent @event);

    public async Task CompleteAsync()
    {
        if (State.Status == SagaStatus.Completed) return;

        if (_deleteUponCompletion)
        {
            await StateProvider.DeleteAsync(State.CorrelationId);
            return;
        }

        State.Status = SagaStatus.Completed;
        await StateProvider.SaveAsync(State.CorrelationId, State);
    }

    public bool IsActive()
    {
        return State.Status == SagaStatus.Active;
    }

    public bool IsCompleted()
    {
        return State.Status == SagaStatus.Completed;
    }

    public bool IsSubscribedTo(IEvent @event) => _handlers.ContainsKey(@event.GetType());
    
    public void RegisterHandler<TEvent>(Action<TEvent> handler)
        where TEvent : IEvent
    {
        _handlers[typeof(TEvent)] = (e) => handler((TEvent)e);
    }

    private void Start(Guid correlationId)
    {
        State.CorrelationId = correlationId;
        State.Status = SagaStatus.Active;
    }

    private async Task<bool> ApplyAsync(IEvent @event)
    {
        var payload = State.Payload.ToObject<TState>();

        var eventType = @event.GetType();
        if (_handlers.TryGetValue(eventType, out Action<IEvent>? handler))
        {
            handler(@event);

            State.Payload = JObject.FromObject(payload);
        }

        return await StateProvider.SaveAsync(State.CorrelationId, State);
    }

    private async Task<TState> LoadStateAsync(Guid correlationId)
    {
        return (TState)await StateProvider.GetAsync(correlationId);
    }
}