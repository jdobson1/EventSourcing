using System.Collections.ObjectModel;

namespace Core.Domain
{
    public abstract class AggregateBase : IAggregate
    {
        private readonly List<IEvent> _events = new();
        public ReadOnlyCollection<IEvent> Events => _events.AsReadOnly();
        public Guid Id { get; protected set; }
        public int Version { get; protected set; }

        protected AggregateBase(Guid id)
        {
            Id = id;
        }

        protected AggregateBase()
        { }

        public void Apply(IEvent @event)
        {
            _events.Add(@event);
        }

        public void Apply(IEnumerable<IEvent> events)
        {
            foreach (var @event in events)
            {
                Mutate(@event);
                Version += 1;
            }
        }

        protected abstract void Mutate(IEvent @event);
    }
}