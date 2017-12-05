using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventStore
{
    public class MemoryEventStore : IEventStore
    {
        Dictionary<Guid, string> events = new Dictionary<Guid, string>();
        private Dictionary<Type, dynamic> indexes = new Dictionary<Type, dynamic>();

        public Task<Event> Save(Event @event)
        {
            var storedEvent = @event.Copy();
            storedEvent.Id = Guid.NewGuid();
            events.Add(storedEvent.Id, JsonConvert.SerializeObject(storedEvent));
            return Task.FromResult(storedEvent);
        }

        public Task<T> Get<T>(Guid id) where T : Event
        {
            T deserializedObject = JsonConvert.DeserializeObject<T>(events[id]);
            return Task.FromResult(deserializedObject);
        }

        public Task<IEnumerable<Event>> Get(IEnumerable<Guid> ids)
        {
            var serializedEvents = events
                .Where(e => ids.Contains(e.Key))
                .Select(e => e.Value);
            var partiallyParsedEvents = serializedEvents
                .Select(e => JObject.Parse(e))
                .ToList();

            IEnumerable<Event> parsedEvents = partiallyParsedEvents
                .Select(e => e.TransformAndMaterialize())
                .ToList();

            return Task.FromResult(parsedEvents);
        }

        public Task Index<T>(Event @event)
        {
            var indexType = typeof(T);
            if (!indexes.ContainsKey(indexType))
            {
                var newIndexTypeInfo = typeof(Index<>).MakeGenericType(indexType);
                indexes.Add(indexType, Activator.CreateInstance(newIndexTypeInfo));
            }
            var index = indexes[indexType];
            index.Add(@event);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Event>> GetByIndex<T>(T key)
        {
            var indexType = typeof(T);
            var index = indexes[indexType];
            IEnumerable<Guid> eventIds = index.GetByKey(key);
            return Get(eventIds);
        }

        public Task<IEnumerable<Event>> GetByIndexKeys<T>(IEnumerable<T> keys)
        {
            var indexType = typeof(T);
            var index = indexes[indexType];
            IEnumerable<Guid> eventIds = keys.SelectMany<T, Guid>(k => index.GetByKey(k));
            return Get(eventIds);
        }
    }
}
