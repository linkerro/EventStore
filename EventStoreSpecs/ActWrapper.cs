using EventStore;
using System.Threading.Tasks;
using System;

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
            public ActAsync ActAsync;

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

            public static ActWrapper From(ActAsync actAsync)
            {
                var actWrapper = new ActWrapper();
                actWrapper.HasBeenCalled = false;
                actWrapper.ActTask = new Task(() => { });
                actWrapper.ActAsync = (e, context) =>
                {
                    actWrapper.Event = e;
                    actWrapper.PipelineContext = context;
                    actWrapper.HasBeenCalled = true;
                    actAsync(e, context);
                    if (!actWrapper.ActTask.IsCompleted)
                    {
                        actWrapper.ActTask.RunSynchronously();
                    }
                    return Task.CompletedTask;
                };
                return actWrapper;
            }
        }
    }
}
