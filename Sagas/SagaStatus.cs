using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Sagas;

[JsonConverter(typeof(StringEnumConverter))]
public enum SagaStatus
{
    [EnumMember(Value = "active")]
    Active,
    [EnumMember(Value = "completed")]
    Completed
}