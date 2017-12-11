using EventStore;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStoreSpecs
{
    [TestClass]
    public partial class DispatcherSpecs
    {
        private MemoryEventStore eventStore;
        private Dispatcher dispatcher;
        private Act nopAct = (e, context) => { };

        [TestInitialize]
        public void TestInitialization()
        {
            eventStore = new MemoryEventStore();

            dispatcher = new Dispatcher(eventStore, null);
        }

        [TestMethod]
        public void DispatcherShouldRegisterPipeline()
        {
            var eventTypes = new List<Type>
            {
                typeof(Event1)
            };
            var actions = new List<Act> { nopAct };
            Action action = () => dispatcher.RegisterPipeline(eventTypes, actions);
            action.ShouldNotThrow();
        }

        [TestMethod]
        public async Task DispatcherShouldDispatchEventToPipelineAsync()
        {
            var actWrapper = ActWrapper.From(nopAct);
            var actions = new List<Act> { actWrapper.Act };
            var eventTypes = new List<Type>
            {
                typeof(Event1)
            };
            dispatcher.RegisterPipeline(eventTypes, actions);
            dispatcher.Dispatch(new Event1());
            await actWrapper.ActTask;
            actWrapper.HasBeenCalled.Should().BeTrue();
        }

        [TestMethod]
        public async Task DispatcherShouldRegisterMultiplePipelinesPerEventAsync()
        {
            var actWrapperForPipe1 = ActWrapper.From(nopAct);
            var actWrapperForPipe2 = ActWrapper.From(nopAct);

            var eventTypes = new List<Type> { typeof(Event1) };
            var actsForPipe1 = new List<Act> { actWrapperForPipe1.Act };
            var actsForPipe2 = new List<Act> { actWrapperForPipe2.Act };
            dispatcher.RegisterPipeline(eventTypes, actsForPipe1);
            dispatcher.RegisterPipeline(eventTypes, actsForPipe2);

            dispatcher.Dispatch(new Event1());
            await Task.WhenAll(new Task[] { actWrapperForPipe1.ActTask, actWrapperForPipe2.ActTask });

            actWrapperForPipe1.HasBeenCalled.Should().BeTrue();
            actWrapperForPipe2.HasBeenCalled.Should().BeTrue();
        }

        [TestMethod]
        public async Task DispatcherShouldRegisterPipelineForMultipleEventsAsync()
        {
            var actWrapper = ActWrapper.From(nopAct);

            var eventTypes = new List<Type> { typeof(Event1), typeof(Event2) };
            var acts = new List<Act> { actWrapper.Act };
            dispatcher.RegisterPipeline(eventTypes, acts);

            Event1 event1 = new Event1 { Property = "event1" };
            dispatcher.Dispatch(event1);
            await actWrapper.ActTask;

            (actWrapper.Event as Event1).Property.ShouldBeEquivalentTo(event1.Property);
            Event2 event2 = new Event2 { Property = "event2" };
            dispatcher.Dispatch(event2);
            await actWrapper.ActTask;

            (actWrapper.Event as Event2).Property.ShouldBeEquivalentTo(event2.Property);
        }

        [TestMethod]
        public async Task DispatcherShouldSaveEventAsync()
        {
            var actWrapper = ActWrapper.From(nopAct);

            var eventTypes = new List<Type> { typeof(Event1) };
            var acts = new List<Act> { actWrapper.Act };
            dispatcher.RegisterPipeline(eventTypes, acts);

            var originalEvent = new Event1();
            dispatcher.Dispatch(originalEvent);
            await actWrapper.ActTask;

            var savedEvent = await eventStore.Get<Event1>(actWrapper.Event.Id);
            savedEvent.Should().NotBeNull();
        }

        class Event1 : Event
        {
            public string Property { get; set; }
        }

        class Event2 : Event
        {
            public string Property { get; set; }
        }
    }
}
