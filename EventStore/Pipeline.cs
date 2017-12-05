using System;
using System.Collections.Generic;

namespace EventStore
{
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
}
