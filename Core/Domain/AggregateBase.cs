using EventStore;
using System.Collections.ObjectModel;

namespace Core.Domain
{
    public abstract class AggregateBase : IAggregate
    {
        private List<IEvent> _events = new List<IEvent>();
        public ReadOnlyCollection<IEvent> Events => _events.AsReadOnly();
        public Guid Id { get; protected set; }
        public int Version { get; private set; }

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