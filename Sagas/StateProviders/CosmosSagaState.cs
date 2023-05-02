namespace Sagas.StateProviders;

public class CosmosSagaState : SagaState
{
    [JsonProperty("_etag")]
    public string Etag { get; set; }
}