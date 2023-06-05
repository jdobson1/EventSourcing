using Core.Domain;
using EventStore;

namespace Sagas;

public interface ISagaEngine
{
    void RegisterSaga(Saga saga);
    Task HandleEventAsync(IReadOnlyCollection<Change> changes);
}