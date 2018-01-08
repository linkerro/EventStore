using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStore
{
    public class Pipeline
    {
        private IEnumerable<Type> handledEventTypes;
        private ActList acts;
        private IDispatcher dispatcher;
        private IReconciliationService reconciliationService;
        private IEventStore eventStore;

        public Pipeline(IEnumerable<Type> handledEventTypes, ActList acts, IDispatcher dispatcher, IReconciliationService reconciliationService, IEventStore eventStore)
        {
            this.handledEventTypes = handledEventTypes;
            this.acts = acts;
            this.dispatcher = dispatcher;
            this.reconciliationService = reconciliationService;
            this.eventStore = eventStore;
        }

        public async Task FireEvent(Event @event)
        {
            foreach (var act in acts)
            {
                var context = new PipelineContext
                {
                    Dispathcer = dispatcher,
                    ReconciliationService = reconciliationService,
                    EventStore=eventStore
                };
                await act(@event, context);
            }
        }
    }
}
