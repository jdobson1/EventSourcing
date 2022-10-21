namespace Core.Domain
{
    public interface IEvent
    {
        DateTime Timestamp { get; }
        string UserId { get; set; }
    }
}