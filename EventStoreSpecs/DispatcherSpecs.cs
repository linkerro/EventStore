using EventStore;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace EventStoreSpecs
{
    [TestClass]
    public partial class DispatcherSpecs
    {
        private Dispatcher dispatcher;
        private Act nopAct = (e, context) => { };

        [TestInitialize]
        public void TestInitialization()
        {
            IEventStore eventStore = new MemoryEventStore();

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
        public void DispatcherShouldDispatchEventToPipeline()
        {
            var actWrapper = ActWrapper.From(nopAct);
            var actions = new List<Act> { actWrapper.Act };
            var eventTypes = new List<Type>
            {
                typeof(Event1)
            };
            dispatcher.RegisterPipeline(eventTypes, actions);
            dispatcher.Dispatch(new Event1());
            actWrapper.HasBeenCalled.Should().BeTrue();
        }

        [TestMethod]
        public void DispatcherShouldRegisterMultiplePipelinesPerEvent()
        {
            var actWrapperForPipe1 = ActWrapper.From(nopAct);
            var actWrapperForPipe2 = ActWrapper.From(nopAct);

            var eventTypes = new List<Type> { typeof(Event1) };
            var actsForPipe1 = new List<Act> { actWrapperForPipe1.Act };
            var actsForPipe2 = new List<Act> { actWrapperForPipe2.Act };
            dispatcher.RegisterPipeline(eventTypes, actsForPipe1);
            dispatcher.RegisterPipeline(eventTypes, actsForPipe2);

            dispatcher.Dispatch(new Event1());
            actWrapperForPipe1.HasBeenCalled.Should().BeTrue();
            actWrapperForPipe2.HasBeenCalled.Should().BeTrue();
        }

        [TestMethod]
        public void DispatcherShouldRegisterPipelineForMultipleEvents()
        {
            var actWrapper = ActWrapper.From(nopAct);

            var eventTypes = new List<Type> { typeof(Event1),typeof(Event2) };
            var acts = new List<Act> { actWrapper.Act };
            dispatcher.RegisterPipeline(eventTypes, acts);

            Event1 event1 = new Event1();
            dispatcher.Dispatch(event1);
            actWrapper.Event.ShouldBeEquivalentTo(event1);
            Event2 event2 = new Event2();
            dispatcher.Dispatch(event2);
            actWrapper.Event.ShouldBeEquivalentTo(event2);
        }

        class Event1 : Event
        {
            
        }

        class Event2 : Event
        {

        }
    }
}
