using System;
using System.Collections.ObjectModel;

namespace Core.Domain
{
    public interface IAggregate
    {
        Guid Id { get; }
        ReadOnlyCollection<IEvent> Events { get; }
        int Version { get; }
    }
}