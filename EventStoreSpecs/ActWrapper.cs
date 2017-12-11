using EventStore;
using System.Threading.Tasks;

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
            public Task ActTask;

            public static ActWrapper From(Act act)
            {
                var actWrapper = new ActWrapper();
                actWrapper.HasBeenCalled = false;
                actWrapper.ActTask = new Task(() => { });
                actWrapper.Act = (e, context) =>
                {
                    actWrapper.Event = e;
                    actWrapper.PipelineContext = context;
                    actWrapper.HasBeenCalled = true;
                    act(e, context);
                    if (!actWrapper.ActTask.IsCompleted)
                    {
                        actWrapper.ActTask.RunSynchronously();
                    }
                };
                return actWrapper;
            }
        }
    }
}
