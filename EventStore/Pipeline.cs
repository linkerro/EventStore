using System;
using System.Collections.Generic;

namespace EventStore
{
    public class Pipeline
    {
        public IEnumerable<Type> HandledEventTypes;
        public IEnumerable<Act> Acts;
        public IDispatcher dispatcher;

        public void FireEvent(Event @event)
        {
            foreach (var act in Acts)
            {
                var context = new PipelineContext { Dispathcer = dispatcher };
                act(@event, context);
            }
        }
    }
}
