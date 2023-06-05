using System.Linq.Expressions;
using Core.Domain;

namespace Sagas;

public abstract class Saga
{
    protected readonly ISagaStateProvider StateProvider;
    protected List<SagaLocatorConfiguration> LocatorConfigurations = new();
    protected List<SagaEvent> SagaEvents = new();

    protected Saga(ISagaStateProvider stateProvider)
    {
        StateProvider = stateProvider;
    }

    protected internal abstract void ConfigureSagaEventCorrelation(IConfigureSagaToEventCorrelation sagaToEventCorrelation);
    protected abstract Task CompleteAsync();
    internal abstract Task<bool> HandleAsync(IEvent @event);
    internal abstract bool IsSubscribedTo(IEvent @event);

    internal void Initialize()
    {
        var correlator = new SagaCorrelator();
        ConfigureSagaEventCorrelation(correlator);
       
        var sagaType = GetType();
        var sagaStateType = sagaType.BaseType?.GenericTypeArguments.FirstOrDefault();

        if (sagaStateType == null) throw new Exception($"Saga {sagaType.Name} must specify a state type");

        SagaEvents = GetAssociatedEvents(sagaType)
            .ToList();

        ///todo: validate associated events

        LocatorConfigurations = correlator.Mappings;
        
        foreach (var associatedEvent in SagaEvents)
        {
            if (associatedEvent.CanStartSaga)
            {
                RegisterHandler(associatedEvent.EventType, GetMethod(sagaType, associatedEvent.EventType, typeof(IAmStartedByEvents<>)));
                continue;
            }

            RegisterHandler(associatedEvent.EventType, GetMethod(sagaType, associatedEvent.EventType, typeof(IHandleEvents<>)));
        }
    }
    internal abstract void RegisterHandler(Type eventType, Func<object, object, Task> handler);

    protected static Func<object, object, Task> GetMethod(Type targetType, Type messageType, Type interfaceGenericType)
    {
        var interfaceType = interfaceGenericType.MakeGenericType(messageType);

        if (!interfaceType.IsAssignableFrom(targetType)) return null;
        
        var methodInfo = targetType.GetInterfaceMap(interfaceType).TargetMethods.FirstOrDefault();
        if (methodInfo == null) return null;
        
        var target = Expression.Parameter(typeof(object));
        var eventParam = Expression.Parameter(typeof(object));
        var castTarget = Expression.Convert(target, targetType);
        var methodParameters = methodInfo.GetParameters();
        var eventCastParam = Expression.Convert(eventParam, methodParameters.ElementAt(0).ParameterType);

        Expression body = Expression.Call(castTarget, methodInfo, eventCastParam);

        return Expression.Lambda<Func<object, object, Task>>(body, target, eventParam).Compile();
    }

    protected static IEnumerable<SagaEvent> GetAssociatedEvents(Type sagaType)
    {
        var result = GetEventsCorrespondingToFilterOnSaga(sagaType, typeof(IAmStartedByEvents<>))
            .Select(t => new SagaEvent(t, true)).ToList();

        foreach (var messageType in GetEventsCorrespondingToFilterOnSaga(sagaType, typeof(IHandleEvents<>)))
        {
            if (result.Any(m => m.EventType == messageType))
            {
                continue;
            }
            result.Add(new SagaEvent(messageType, false));
        }

        return result;
    }

    protected static IEnumerable<Type> GetEventsCorrespondingToFilterOnSaga(Type sagaType, Type filter)
    {
        foreach (var interfaceType in sagaType.GetInterfaces())
        {
            foreach (var argument in interfaceType.GetGenericArguments())
            {
                var genericType = filter.MakeGenericType(argument);
                var isOfFilterType = genericType == interfaceType;
                if (!isOfFilterType)
                {
                    continue;
                }
                yield return argument;
            }
        }
    }
}

public abstract class Saga<TSagaState> : Saga where TSagaState : class, ISagaState, new()
{
    public TSagaState State;
    private readonly Dictionary<Type, Func<object, object, Task>?> _handlers;
    private readonly bool _deleteUponCompletion;

    protected Saga(ISagaStateProvider stateProvider, bool deleteUponCompletion = false) : base(stateProvider)
    {
        _deleteUponCompletion = deleteUponCompletion;
        _handlers = new Dictionary<Type, Func<object, object, Task>?>();
    }

    protected internal override void ConfigureSagaEventCorrelation(IConfigureSagaToEventCorrelation sagaToEventCorrelation)
    {
        ConfigureSagaEventCorrelation(new SagaPropertyMapper<TSagaState>(sagaToEventCorrelation));
    }

    public abstract void ConfigureSagaEventCorrelation(SagaPropertyMapper<TSagaState> sagaPropertyMapper);

    internal override async Task<bool> HandleAsync(IEvent @event)
    {
        var locator = LocatorConfigurations.FirstOrDefault(x => x.EventType == @event.GetType());

        if (locator == null) return true;

        var eventPropertyValue = (Guid)(locator?.GetEventPropertyValue(@event) ??
                                        throw new Exception(
                                            $"Value for event {locator?.EventTypeName} correlation property is null for saga {GetType().Name}"));
        
        var state = await GetSagaState(eventPropertyValue) as TSagaState;

        if (state == null || state.Id == Guid.Empty)
        {
            // can this event start the saga?
            var canStartSaga = SagaEvents.Any(x => x.CanStartSaga && x.EventType == @event.GetType());

            if (!canStartSaga) return true;

            State = Activator.CreateInstance<TSagaState>();
            State.Id = eventPropertyValue;
        }
        else
        {
            State = state;
        }

        if (State.Completed) return true;

        return await ApplyAsync(@event);
    }

    protected override async Task CompleteAsync()
    {
        if (State.Completed) return;

        if (_deleteUponCompletion)
        {
            await StateProvider.DeleteAsync<TSagaState>(State.Id);
            return;
        }

        State.MarkCompleted();
        await StateProvider.SaveAsync(State.Id, State);
    }

    internal override bool IsSubscribedTo(IEvent @event) => _handlers.ContainsKey(@event.GetType());
    
    internal override void RegisterHandler(Type eventType, Func<object, object, Task> handler)
    {
        _handlers[eventType] = handler;
    }

    private async Task<bool> ApplyAsync(IEvent @event)
    {
        var eventType = @event.GetType();

        if (_handlers.TryGetValue(eventType, out Func<object, object, Task>? handler))
        {
            if (handler == null)
                throw new Exception($"Handler for event type {eventType} not found on saga {GetType()}");
            await handler.Invoke(this, @event);
            
            return await StateProvider.SaveAsync(State.Id, State);
        }

        return false;
    }

    private async Task<ISagaState> GetSagaState(Guid id)
    {
        return await StateProvider.GetAsync<TSagaState>(id);
    }
}