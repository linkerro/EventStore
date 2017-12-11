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

        public Pipeline(IEnumerable<Type> handledEventTypes, IEnumerable<Act> acts, IDispatcher dispatcher, IReconciliationService reconciliationService)
        {
            this.handledEventTypes = handledEventTypes;
            this.acts = acts;
            this.dispatcher = dispatcher;
            this.reconciliationService = reconciliationService;
        }

        public void FireEvent(Event @event)
        {
            foreach (var act in acts)
            {
                var context = new PipelineContext
                {
                    Dispathcer = dispatcher,
                    ReconciliationService = reconciliationService
                };
                act(@event, context);
            }
        }
    }
}
