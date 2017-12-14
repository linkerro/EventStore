using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventStore
{
    public class Index<T>
    {
        Dictionary<T, List<Guid>> dictionary = new Dictionary<T, List<Guid>>();

        public void Add(Event @event)
        {
            var key = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(@event));
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, new List<Guid>());
            }
            dictionary[key].Add(@event.Id);
        }

        public IEnumerable<Guid> GetByKey(T key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            return new Guid[0];
        }

        public IEnumerable<Guid> GetByMatcher(object lookupTerm)
        {
            var lookupType = lookupTerm.GetType();
            var serializedLookupTerm = JsonConvert.SerializeObject(lookupTerm);
            var results = dictionary.Where(keyValuePair =>
            {
                var keyMatcher = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(JsonConvert.SerializeObject(keyValuePair.Key), lookupType));
                return keyMatcher == serializedLookupTerm;
            })
            .SelectMany(keyValuePair => keyValuePair.Value)
            .ToArray();
            return results;
        }

        public IEnumerable<Guid> GetAll()
        {
            return dictionary.SelectMany(d => d.Value).ToArray();
        }
    }
}
