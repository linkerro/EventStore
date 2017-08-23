using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventStore
{
    public class Index<T> where T : struct
    {
        Type type;
        Dictionary<T, List<Guid>> index = new Dictionary<T, List<Guid>>();

        public void Add(IEvent @event)
        {
            var key = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(@event));
            if (!index.ContainsKey(key))
            {
                index.Add(key, new List<Guid>());
            }
            index[key].Add(@event.Id);
        }

        public IEnumerable<Guid> GetByKey(T key)
        {
            return index
                .SingleOrDefault(i => i.Key.Equals(key))
                .Value;
        }
    }
}
