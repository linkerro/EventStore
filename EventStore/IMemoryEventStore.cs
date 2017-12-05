using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStore
{
    public interface IEventStore
    {
        Task<IEnumerable<Event>> Get(IEnumerable<Guid> ids);
        Task<T> Get<T>(Guid id) where T : Event;
        Task<IEnumerable<Event>> GetByIndex<T>(T key);
        Task<IEnumerable<Event>> GetByIndexKeys<T>(IEnumerable<T> keys);
        Task Index<T>(Event @event);
        Task<Event> Save(Event @event);
    }
}