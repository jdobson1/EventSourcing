using Core.Domain;
using Newtonsoft.Json.Linq;

namespace Sagas
{
    /// <summary>
    /// Platform-level message used for service to service communication. The message can be a command or an event.
    /// </summary>
    public class PlatformMessage
    {
        /// <summary>
        /// Message type name.
        /// </summary>
        public string TypeName { get; set; } = null!;

        /// <summary>
        /// Message body.
        /// </summary>
        public string Message { get; set; } = null!;

        public PlatformMessage(string typeName, string message)
        {
            TypeName = typeName;
            Message = message;
        }

        public PlatformMessage()
        {   
        }

        public IEvent GetEvent(IEventTypeResolver eventTypeResolver)
        {
            Type eventType = eventTypeResolver.GetEventType(TypeName);
            
            return (IEvent)new JObject(Message).ToObject(eventType);
        }
    }
}
