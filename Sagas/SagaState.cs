using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sagas;

public class SagaState : ISagaState
{
    public SagaState()
        : this(new JObject())
    {
    }

    public SagaState(JObject payload)
    {
        Payload = payload;
    }

    [JsonProperty("id")]
    public Guid CorrelationId { get; set; }

    [JsonProperty("status")]
    public SagaStatus Status { get; set; }

    [JsonProperty("payload")]
    public JObject Payload { get; set; }
}