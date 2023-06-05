using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Sagas;

public class SagaState : ISagaState
{
    //public SagaState()
    //    : this(new JObject())
    //{
    //}

    //public SagaState(JObject payload)
    //{
    //    Payload = payload;
    //}

    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("completed")]
    public bool Completed { get; internal set; }

    void ISagaState.MarkCompleted()
    {
        Completed = true;
    }
}

public class SagaStateWrapper
{

    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("payload")]
    public JObject Payload { get; set; }

    [JsonProperty("_etag")]
    public string Etag { get; set; }

    
}