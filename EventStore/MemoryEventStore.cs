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
        private Dictionary<string, dynamic> indexes = new Dictionary<string, dynamic>();

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

        public Task Index(Event @event, string indexName, object indexKey)
        {
            var indexKeyType = indexKey.GetType();
            var indexTypeInfo = typeof(Index<>).MakeGenericType(indexKeyType);

            if (!indexes.ContainsKey(indexName))
            {
                indexes.Add(indexName, Activator.CreateInstance(indexTypeInfo));
            }
            dynamic index = indexes[indexName];
            indexTypeInfo.GetMethod(nameof(Index<object>.Add)).Invoke(index, new object[] { @event });
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Event>> GetByIndex(string indexName)
        {
            var indexType = (Type)indexes[indexName].GetType();
            var getAllName = nameof(Index<object>.GetAll);
            var ids = (IEnumerable<Guid>)indexType.GetMethod(getAllName).Invoke(indexes[indexName], null);
            return Get(ids);
        }

        public Task<IEnumerable<Event>> GetFromIndex(string indexName, object lookupTerm)
        {
            var indexType = (Type)indexes[indexName].GetType();
            var getByMatcherName = nameof(Index<object>.GetByMatcher);
            var ids = (IEnumerable<Guid>)indexType.GetMethod(getByMatcherName).Invoke(indexes[indexName], new object[] { lookupTerm });
            return Get(ids);
        }
    }
}
