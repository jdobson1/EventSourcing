namespace Sagas;

public class SagaEvent
{
    internal SagaEvent(Type eventType, bool canStart)
    {
        EventType = eventType;
        EventTypeName = eventType.FullName;
        CanStartSaga = canStart;
    }
    
    public Type EventType { get; }
    
    public string? EventTypeName { get; }
    
    public bool CanStartSaga { get; }
}