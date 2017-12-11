using EventStore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
        private Mock<IReconciliationService> reconciliationServiceMock;
        private ActWrapper actWrapper;
        private Pipeline pipeline;

        [TestInitialize]
        public void Initialize()
        {
            dispatcherMock = new Mock<IDispatcher>(MockBehavior.Strict);
            reconciliationServiceMock = new Mock<IReconciliationService>(MockBehavior.Strict);
            actWrapper = ActWrapper.From(nopAct);
            pipeline = new Pipeline(
                new List<Type> { typeof(Event) },
                new List<Act> { actWrapper.Act },
                dispatcherMock.Object,
                reconciliationServiceMock.Object
                );
        }

        [TestMethod]
        public void Pipeline_ShouldFireEvents()
        {
            var @event = new Event();
            pipeline.FireEvent(@event);
            actWrapper.HasBeenCalled.Should().BeTrue();
        }

        [TestMethod]
        public void Pipeline_ShouldPopulateTheContextWithTheDispatcher()
        {
            var @event = new Event();
            pipeline.FireEvent(@event);

            actWrapper.PipelineContext.Dispathcer.ShouldBeEquivalentTo(dispatcherMock.Object);
        }

        [TestMethod]
        public void Pipeline_ShouldPopulateTheReconciliationServiceField()
        {
            var @event = new Event();
            pipeline.FireEvent(@event);

            actWrapper.PipelineContext.ReconciliationService.ShouldBeEquivalentTo(reconciliationServiceMock.Object);
        }
    }
}
