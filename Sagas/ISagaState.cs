using Newtonsoft.Json.Linq;

namespace Sagas;

public interface ISagaState
{
    Guid CorrelationId { get; set; }
    SagaStatus Status { get; set; }
    JObject Payload { get; set; }
}