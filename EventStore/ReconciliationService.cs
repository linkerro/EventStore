using EventStore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStore
{
    public class ReconciliationService : IReconciliationService
    {
        private Dictionary<Guid,ReconciliationInfo> reconciliationTable=new Dictionary<Guid, ReconciliationInfo>();

        public Task<Event> GetReconciliationTask(Guid reconciliationId)
        {
            var reconciliationInfo = new ReconciliationInfo();
            reconciliationInfo.Task = new Task<Event>(() => reconciliationInfo.Result);
            reconciliationTable.Add(reconciliationId, reconciliationInfo);
            return reconciliationInfo.Task;
        }

        public void ResolveTask(Event @event)
        {
            switch (@event)
            {
                case ReconciliationEvent reconciliationEvent:
                    var reconciliationId = reconciliationEvent.ReconciliationId;
                    if (reconciliationTable.ContainsKey(reconciliationId))
                    {
                        var reconciliationInfo = reconciliationTable[reconciliationId];
                        reconciliationInfo.Result = @event;
                        reconciliationInfo.Task.RunSynchronously();
                    }
                break;
            }
        }
    }

    class ReconciliationInfo
    {
        public Task<Event> Task { get; set; }
        public Event Result { get; set; }
    }
}
