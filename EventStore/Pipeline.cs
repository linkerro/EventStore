using System;
using System.Collections.Generic;

namespace EventStore
{
    public class Pipeline
    {
        private IEnumerable<Type> handledEventTypes;
        private IEnumerable<Act> acts;
        private IDispatcher dispatcher;
        private IReconciliationService reconciliationService;
        private IEventStore eventStore;

        public Pipeline(IEnumerable<Type> handledEventTypes, IEnumerable<Act> acts, IDispatcher dispatcher, IReconciliationService reconciliationService, IEventStore eventStore)
        {
            this.handledEventTypes = handledEventTypes;
            this.acts = acts;
            this.dispatcher = dispatcher;
            this.reconciliationService = reconciliationService;
            this.eventStore = eventStore;
        }

        public void FireEvent(Event @event)
        {
            foreach (var act in acts)
            {
                var context = new PipelineContext
                {
                    Dispathcer = dispatcher,
                    ReconciliationService = reconciliationService,
                    EventStore=eventStore
                };
                act(@event, context);
            }
        }
    }
}
