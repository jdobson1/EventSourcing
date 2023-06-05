namespace Sagas;

public interface ISagaLocator<TSagaState, in TEvent>
{
    Task<TSagaState> Locate(TEvent @event);
}