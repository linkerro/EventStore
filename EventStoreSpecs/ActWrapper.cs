using EventStore;

namespace EventStoreSpecs
{
    public partial class DispatcherSpecs
    {
        public class ActWrapper
        {
            public Act Act;
            public bool HasBeenCalled;
            public PipelineContext PipelineContext;
            public Event Event;

            public static ActWrapper From(Act act)
            {
                var actWrapper = new ActWrapper();
                actWrapper.HasBeenCalled = false;
                actWrapper.Act = (e, context) =>
                {
                    actWrapper.Event = e;
                    actWrapper.PipelineContext = context;
                    actWrapper.HasBeenCalled = true;
                    act(e, context);
                };
                return actWrapper;
            }
        }
    }
}
