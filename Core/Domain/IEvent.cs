namespace Core.Domain
{
    public interface IEvent
    {
        DateTime Timestamp { get; }
        string ClientId { get; set; }
    }
}