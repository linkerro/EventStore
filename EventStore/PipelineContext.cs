namespace EventStore
{
    public class PipelineContext
    {
        public IDispatcher Dispathcer { get; set; }
        public IReconciliationService ReconciliationService { get; internal set; }
    }
}
