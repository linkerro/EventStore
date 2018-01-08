using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStore
{
    public interface IDispatcher
    {
        void Dispatch(Event @event);
        void RegisterPipeline(IEnumerable<Type> handledEventTypes, ActList acts);
        Task<Event> DispatchWithReconciliation(ReconciliationEvent reconciliationEvent);
    }
}