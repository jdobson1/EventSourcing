using System.Reflection;
using Core.Domain;

namespace Products.UnitTests;

public class FakeRepository<T> : RepositoryBase<T> where T : AggregateBase
{
    private readonly Dictionary<Guid, List<IEvent>> _eventStore = new();
    public FakeRepository() : base(null)
    {
    }

    public override Task<T> GetById(Guid id, string clientId)
    {
        return Task.FromResult((T)Activator.CreateInstance(typeof(T), BindingFlags.NonPublic | BindingFlags.Instance,
            null, new object[] { _eventStore[id].OrderBy(x => x.Timestamp) }, null)!);
    }

    public override Task<T> GetByIndexedProperty(string indexedPropertyValue)
    {
        throw new NotImplementedException();
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
}