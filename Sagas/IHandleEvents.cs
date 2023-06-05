namespace Sagas;

public interface IHandleEvents<in TEvent>
{
    Task Handle(TEvent @event);
}