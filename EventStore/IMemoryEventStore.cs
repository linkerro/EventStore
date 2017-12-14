using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStore
{
    public interface IEventStore
    {
        Task<IEnumerable<Event>> Get(IEnumerable<Guid> ids);
        Task<T> Get<T>(Guid id) where T : Event;
        Task Index(Event @event, string indexName, object indexKey);
        Task<IEnumerable<Event>> GetByIndex(string indexName);
        Task<IEnumerable<Event>> GetFromIndex(string indexName, object lookupTerm);
        Task<Event> Save(Event @event);
    }
}