using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStore
{
    public class Pipeline
    {
        private IEnumerable<Type> handledEventTypes;
        private IEnumerable<Actor> actors;
        private IDispatcher dispatcher;
        private IReconciliationService reconciliationService;
        private IEventStore eventStore;

        public Pipeline(IEnumerable<Type> handledEventTypes, IEnumerable<Actor> actors, IDispatcher dispatcher, IReconciliationService reconciliationService, IEventStore eventStore)
        {
            this.handledEventTypes = handledEventTypes;
            this.actors = actors;
            this.dispatcher = dispatcher;
            this.reconciliationService = reconciliationService;
            this.eventStore = eventStore;
        }

        public async void FireEvent(Event @event)
        {
            foreach (var actor in actors)
            {
                var context = new PipelineContext
                {
                    Dispathcer = dispatcher,
                    ReconciliationService = reconciliationService,
                    EventStore=eventStore
                };
                if (actor.IsAsync)
                {
                    await actor.ActAsync(@event, context);
                }
                else
                {
                    actor.Act(@event, context);
                }
            }
        }
    }
}
