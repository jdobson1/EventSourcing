namespace Sagas
{
    public interface IAmStartedByEvents<in TEvent>
    {
        Task Handle(TEvent @event);
    }
}
