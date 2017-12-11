using System;
using System.Collections.Generic;
using System.Text;

namespace EventStore
{
    public class ReconciliationEvent:Event
    {
        public Guid ReconciliationId { get; set; }
    }
}
