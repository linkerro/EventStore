using EventStore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using static EventStoreSpecs.DispatcherSpecs;
using FluentAssertions;

namespace EventStoreSpecs
{
    [TestClass]
    public class PipelineSpecs
    {
        private Act nopAct = (e, context) => { };
        private Mock<IDispatcher> dispatcherMock;

        [TestInitialize]
        public void Initialize()
        {
            dispatcherMock = new Mock<IDispatcher>(MockBehavior.Strict);
        }

        [TestMethod]
        public void Pipeline_ShouldFireEvents()
        {
            var pipeline = new Pipeline();
            var actWrapper = ActWrapper.From(nopAct);
            pipeline.Acts = new List<Act> { actWrapper.Act };
            pipeline.dispatcher = dispatcherMock.Object;
            pipeline.HandledEventTypes = new List<Type> { typeof(Event) };
            var @event = new Event();
            pipeline.FireEvent(@event);
            actWrapper.HasBeenCalled.Should().BeTrue();
        }

        [TestMethod]
        public void Pipeline_ShouldPopulateTheContextWithTheDispatcher()
        {
            var pipeline = new Pipeline();
            var actWrapper = ActWrapper.From(nopAct);
            pipeline.Acts = new List<Act> { actWrapper.Act };
            pipeline.dispatcher = dispatcherMock.Object;
            pipeline.HandledEventTypes = new List<Type> { typeof(Event) };
            var @event = new Event();
            pipeline.FireEvent(@event);

            actWrapper.PipelineContext.Dispathcer.ShouldBeEquivalentTo(dispatcherMock.Object);
        }
    }
}
