using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace EventStore
{
    public class MemoryEventStore
    {
        Dictionary<Guid, string> events = new Dictionary<Guid, string>();
        private Dictionary<Type,dynamic> indexes=new Dictionary<Type, dynamic>();

        public IEvent Save(IEvent @event)
        {
            var storedEvent = @event.Copy();
            storedEvent.Id = Guid.NewGuid();
            events.Add(storedEvent.Id, JsonConvert.SerializeObject(storedEvent));
            return storedEvent;
        }

        public T Get<T>(Guid id) where T:IEvent
        {
            return JsonConvert.DeserializeObject<T>(events[id]);
        }

        public IEnumerable<dynamic> Get(IEnumerable<Guid> ids)
        {
            var serializedEvents = events
                .Where(e => ids.Contains(e.Key))
                .Select(e=>e.Value);
            return serializedEvents
                .Select(e => JObject.Parse(e))
                .ToList();
        }

        public void Index<T>(IEvent @event)
        {
            var indexType = typeof(T);
            if (!indexes.ContainsKey(indexType))
            {
                var newIndexTypeInfo = typeof(Index<>).MakeGenericType(indexType);
                indexes.Add(indexType, Activator.CreateInstance(newIndexTypeInfo));
            }
            var index = indexes[indexType];
            index.Add(@event);
        }

        public IEnumerable<dynamic> GetByIndex<T>(T key)
        {
            var indexType = typeof(T);
            var index = indexes[indexType];
            IEnumerable<Guid> eventIds = index.GetByKey(key);
            return Get(eventIds);
        }

        public IEnumerable<dynamic> GetByIndexKeys<T>(IEnumerable<T> keys)
        {
            var indexType = typeof(T);
            var index = indexes[indexType];
            IEnumerable<Guid> eventIds = keys.SelectMany<T,Guid>(k => index.GetByKey(k));
            return Get(eventIds);
        }
    }
}
