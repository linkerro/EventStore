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
                    EventStore = eventStore
                };
                try
                {
                    await (Task)act.DynamicInvoke(new object[] { @event, context });
                }
                catch (Exception ex)
                {

                    var exceptionEvent = new ExceptionEvent
                    {
                        Exception = ex,
                        OriginalEvent = @event
                    };
                    switch (@event)
                    {
                        case ReconciliationEvent reconciliationEvent:
                            exceptionEvent.ReconciliationId = reconciliationEvent.ReconciliationId;
                            reconciliationService.ResolveTask(exceptionEvent);
                            break;
                        default:
                            dispatcher.Dispatch(exceptionEvent);
                            break;
                    }
                    dispatcher.Dispatch(exceptionEvent);
                }
            }
        }
    }
}
