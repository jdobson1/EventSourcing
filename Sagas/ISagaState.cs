using Newtonsoft.Json.Linq;

namespace Sagas;

public interface ISagaState
{
    Guid Id { get; set; }
    bool Completed { get; }
    internal void MarkCompleted();
}