using System;

namespace EventStore
{
    public class ExceptionEvent : ReconciliationEvent
    {
        public Exception Exception { get; set; }
        public Event OriginalEvent { get; set; }
    }
}
