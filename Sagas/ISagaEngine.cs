using Core.Domain;

namespace Sagas;

public interface ISagaEngine
{
    void RegisterSaga(ISaga saga);
    Task HandleEventAsync(IReadOnlyCollection<PlatformMessage> platformMessages);
}