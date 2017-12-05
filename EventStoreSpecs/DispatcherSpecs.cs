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
            IEnumerable<Type> eventTypes = new List<Type>
            {
                typeof(Event1)
            };
            IEnumerable<Act> actions = new List<Act> { nopAct };
            Action action = () => dispatcher.RegisterPipeline(eventTypes, actions);
            action.ShouldNotThrow();
        }

        [TestMethod]
        public void DispatcherShouldDispatchEventToPipeline()
        {
            var actWrapper = ActWrapper.From(nopAct);
            IEnumerable<Act> actions = new List<Act> { actWrapper.Act };
            IEnumerable<Type> eventTypes = new List<Type>
            {
                typeof(Event1)
            };
            dispatcher.RegisterPipeline(eventTypes, actions);
            dispatcher.Dispatch(new Event1());
            actWrapper.HasBeenCalled.Should().BeTrue();
        }

        class Event1 : Event
        {
            
        }
    }
}
