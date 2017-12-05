using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventStore
{
    public delegate void Act(Event @event, PipelineContext context);

    public class Dispatcher
    {

        private IEventStore eventStore;
        private Dictionary<Type, List<Pipeline>> pipelines = new Dictionary<Type, List<Pipeline>>();

        public Dispatcher(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public void RegisterPipeline(IEnumerable<Type> handledEventTypes, IEnumerable<Act> acts)
        {
            var pipeline = new Pipeline
            {
                HandledEventTypes = handledEventTypes,
                Acts = acts
            };

            foreach (var eventType in handledEventTypes)
            {
                if (!pipelines.ContainsKey(eventType))
                {
                    pipelines.Add(eventType, new List<Pipeline>());

                    pipelines[eventType].Add(pipeline);
                }
            }
        }

        public void Dispatch(Event @event)
        {
            var eventType = @event.GetType();
            if (pipelines.ContainsKey(eventType))
            {
                foreach (var pipeline in pipelines[eventType])
                {
                    pipeline.FireEvent(@event);
                }
            }
        }
    }

    class Pipeline
    {
        public IEnumerable<Type> HandledEventTypes;
        public IEnumerable<Act> Acts;

        internal void FireEvent(Event @event)
        {
            foreach (var act in Acts)
            {
                act(@event, new PipelineContext());
            }
        }
    }

    public class PipelineContext
    {

    }
}
