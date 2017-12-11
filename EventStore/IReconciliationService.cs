using System;
using System.Threading.Tasks;
using EventStore;

namespace EventStore
{
    public interface IReconciliationService
    {
        Task<Event> GetReconciliationTask(Guid reconciliationId);
        void ResolveTask(Event @event);
    }
}