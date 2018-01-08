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
            public int threadId;

            public static ActWrapper From(Act act)
            {
                var actWrapper = new ActWrapper();
                actWrapper.HasBeenCalled = false;
                actWrapper.ActTask = new Task(() => {});
                actWrapper.Act = (e, context) =>
                {
                    actWrapper.Event = e;
                    actWrapper.PipelineContext = context;
                    actWrapper.HasBeenCalled = true;
                    act(e, context);
                    actWrapper.ActTask.RunSynchronously();
                };
                return actWrapper;
            }

            public void Rearm()
            {
                HasBeenCalled = false;
                ActTask = new Task(() => { });
            }
        }
    }
}
