using System.Reflection;
using Core.Domain;

namespace Products.UnitTests.Domain;

public class FakeRepository<T> : RepositoryBase<T> where T : AggregateBase
{
    private readonly Dictionary<Guid, List<IEvent>> _eventStore = new();
    public FakeRepository() : base(null!)
    {
    }

    public override Task<T> GetById(Guid id, string clientId)
    {
        return Task.FromResult((T)Activator.CreateInstance(typeof(T), BindingFlags.NonPublic | BindingFlags.Instance,
            null, new object[] { _eventStore[id].OrderBy(x => x.Timestamp) }, null)!);
    }

    public override Task<T> GetByIndexedProperty(string indexedPropertyValue, string clientId)
    {
        throw new NotImplementedException();
    }

    public override Task<T> GetByIndexedProperty(string indexedPropertyValue)
    {
        var indexedEvent =
            _eventStore.Values.SingleOrDefault(x => x.Exists(x => GetIndexedProperty(x) == indexedPropertyValue));

        var aggregateStreamKvp = _eventStore.SingleOrDefault(pair => pair.Value.Contains(indexedEvent?.FirstOrDefault()!));

        return Task.FromResult((T)Activator.CreateInstance(typeof(T), BindingFlags.NonPublic | BindingFlags.Instance,
            null, new object[] { _eventStore[aggregateStreamKvp.Key].OrderBy(x => x.Timestamp) }, null)!);
    }

    protected override Task OnSave(T aggregate, string clientId)
    {
        throw new NotImplementedException();
    }

    public override Task Save(T aggregate, string clientId)
    {
        var hasEvents = _eventStore.ContainsKey(aggregate.Id);

        if (!hasEvents)
            _eventStore.Add(aggregate.Id, aggregate.Events.ToList());
        else
        {
            _eventStore[aggregate.Id].AddRange(aggregate.Events);
        }

        aggregate.Apply(aggregate.Events);

        return Task.CompletedTask;
    }
    private static string GetIndexedProperty(IEvent @event)
    {
        var prop = @event.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.GetCustomAttributes(typeof(EventStore.Index), false).Count() == 1);
        return prop?.GetValue(@event, null)?.ToString() ?? string.Empty;
    }
}