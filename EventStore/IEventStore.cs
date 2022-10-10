using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain;

namespace EventStore
{
    public interface IEventStore
    {
        Task<EventStream> LoadStreamAsync(string clientId, string streamId);

        Task<EventStream> LoadStreamAsync(string clientId, string streamId, int fromVersion);
  
        Task<bool> AppendToStreamAsync(
            string clientId,
            string streamId,
            int expectedVersion,
            IEnumerable<IEvent> events);
    }
}