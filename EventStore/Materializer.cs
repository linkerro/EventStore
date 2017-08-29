using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventStore
{
    public static class  Materializer
    {
        public delegate JObject Transform(JObject @event);

        public static Event TransformAndMaterialize(this JObject @event)
        {
            var eventTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(Event).IsAssignableFrom(type))
                .ToArray();

            var eventTypeName = @event[nameof(Event.Name)].ToString();
            var eventType = eventTypes.Single(e => e.Name == eventTypeName);

            var test = eventType.GetCustomAttributes(false);

            var eventTransformClass = eventType.GetCustomAttributes(typeof(EventVersionTransformsAttribute), false)
                .Select(a=>a as EventVersionTransformsAttribute)
                .SingleOrDefault()?.Type;

            if (eventTransformClass != null)
            {
                var eventTransforms = Activator.CreateInstance(eventTransformClass) as TransformInfo;

                var eventVersion = @event[nameof(Event.Version)].ToObject<int>();

                var selectedTransforms = eventTransforms
                    .Transforms
                    .Where(t => t.Key >= eventVersion)
                    .Select(t => t.Value);

                var transformedEvent = selectedTransforms.Aggregate(@event, (context, transform) => transform(context));

                return (Event)transformedEvent.ToObject(eventType);
            }

            return (Event)@event.ToObject(eventType);
        }
    }

    public class TransformInfo
    {
        public Dictionary<int, Materializer.Transform> Transforms { get; set; }
    }
}
