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

            dispatcher = new Dispatcher(eventStore);
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
            await dispatcher.Dispatch(new Event1());
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

            await dispatcher.Dispatch(new Event1());
            actWrapperForPipe1.HasBeenCalled.Should().BeTrue();
            actWrapperForPipe2.HasBeenCalled.Should().BeTrue();
        }

        [TestMethod]
        public async Task DispatcherShouldRegisterPipelineForMultipleEventsAsync()
        {
            var actWrapper = ActWrapper.From(nopAct);

            var eventTypes = new List<Type> { typeof(Event1),typeof(Event2) };
            var acts = new List<Act> { actWrapper.Act };
            dispatcher.RegisterPipeline(eventTypes, acts);

            Event1 event1 = new Event1 { Property = "event1" };
            await dispatcher.Dispatch(event1);
            (actWrapper.Event as Event1).Property.ShouldBeEquivalentTo(event1.Property);
            Event2 event2 = new Event2 { Property = "event2" };
            await dispatcher.Dispatch(event2);
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
            await dispatcher.Dispatch(originalEvent);

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
