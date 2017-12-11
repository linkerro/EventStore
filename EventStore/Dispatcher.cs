using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStore
{
    public delegate void Act(Event @event, PipelineContext context);

    public class Dispatcher : IDispatcher
    {

        private IEventStore eventStore;
        private IReconciliationService reconciliationService;
        private Dictionary<Type, List<Pipeline>> pipelines = new Dictionary<Type, List<Pipeline>>();

        public Dispatcher(IEventStore eventStore, IReconciliationService reconciliationService)
        {
            this.eventStore = eventStore;
            this.reconciliationService = reconciliationService;
        }

        public void RegisterPipeline(IEnumerable<Type> handledEventTypes, IEnumerable<Act> acts)
        {
            var pipeline = new Pipeline(handledEventTypes, acts, this, reconciliationService);

            foreach (var eventType in handledEventTypes)
            {
                if (!pipelines.ContainsKey(eventType))
                {
                    pipelines.Add(eventType, new List<Pipeline>());
                }
                pipelines[eventType].Add(pipeline);
            }
        }

        public async void Dispatch(Event @event)
        {
            var savedEvent = await eventStore.Save(@event);
            var eventType = @event.GetType();
            if (pipelines.ContainsKey(eventType))
            {
                foreach (var pipeline in pipelines[eventType])
                {
                    pipeline.FireEvent(savedEvent);
                }
            }
        }

        public Task<Event> DispatchWithReconciliation(ReconciliationEvent reconciliationEvent)
        {
            var reconciliationId = Guid.NewGuid();
            reconciliationEvent.ReconciliationId = reconciliationId;
            var reconciliationTask = reconciliationService.GetReconciliationTask(reconciliationId);
            Dispatch(reconciliationEvent);
            return reconciliationTask;
        }
    }
}
