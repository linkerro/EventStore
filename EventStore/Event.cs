using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventStore
{
    public interface IEvent
    {
        Guid Id { get; set; }
    }

    public static class EventExtensions
    {
        public static T Copy<T>(this IEvent @event)where T:IEvent
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(@event));
        }

        public static IEvent Copy(this IEvent @event)
        {
            return (IEvent)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(@event), @event.GetType());
        }
    }
}
