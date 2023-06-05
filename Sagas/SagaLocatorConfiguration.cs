using Core.Domain;

namespace Sagas;

public class SagaLocatorConfiguration
{
    public SagaLocatorConfiguration(Type eventType, string eventTypeName, Func<object, object> eventProperty, string sagaPropertyName, Type sagaPropertyType)
    {
        EventType = eventType;
        EventTypeName = eventTypeName;
        EventProperty = eventProperty;
        SagaPropertyName = sagaPropertyName;
        SagaPropertyType = sagaPropertyType;
    }

    public Type EventType { get; set; }
    public string EventTypeName { get; set; }
    public Func<object, object> EventProperty { get; set; }
    public string SagaPropertyName { get; set; }
    public Type SagaPropertyType { get; set; }

    public object GetEventPropertyValue(IEvent @event) => EventProperty(@event);
}