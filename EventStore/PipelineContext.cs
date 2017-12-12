namespace EventStore
{
    public class PipelineContext
    {
        public IDispatcher Dispathcer { get; set; }
        public IReconciliationService ReconciliationService { get; set; }
        public IEventStore EventStore { get; set; }
    }
}
