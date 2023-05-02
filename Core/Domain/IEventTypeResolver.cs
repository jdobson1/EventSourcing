namespace Core.Domain
{
    public interface IEventTypeResolver
    {
        Type GetEventType(string typeName);
    }
}