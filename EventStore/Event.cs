using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventStore
{
    public class Event
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public string Name
        {
            get
            {
                return GetType().Name;
            }
        }
    }

    public static class EventExtensions
    {
        public static T Copy<T>(this Event @event) where T : Event
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(@event));
        }

        public static Event Copy(this Event @event)
        {
            return (Event)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(@event), @event.GetType());
        }
    }
}
