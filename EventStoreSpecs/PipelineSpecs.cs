using EventStore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Moq;
using static EventStoreSpecs.DispatcherSpecs;
using FluentAssertions;
using System.Threading.Tasks;

namespace EventStoreSpecs
{
    [TestClass]
    public class PipelineSpecs
    {
        private Act nopAct = (e, context) => { };
        private Mock<IDispatcher> dispatcherMock;
        private Mock<IReconciliationService> reconciliationServiceMock;
        private Mock<IEventStore> eventStoreMock;
        private ActWrapper actWrapper;
        private Pipeline pipeline;

        [TestInitialize]
        public void Initialize()
        {
            dispatcherMock = new Mock<IDispatcher>(MockBehavior.Strict);
            reconciliationServiceMock = new Mock<IReconciliationService>(MockBehavior.Strict);
            eventStoreMock = new Mock<IEventStore>(MockBehavior.Strict);
            actWrapper = ActWrapper.From(nopAct);
            pipeline = new Pipeline(
                new List<Type> { typeof(Event) },
                new ActList { actWrapper.Act },
                dispatcherMock.Object,
                reconciliationServiceMock.Object,
                eventStoreMock.Object
                );
        }

        [TestMethod]
        public async Task Pipeline_ShouldFireEvents()
        {
            var @event = new Event();
            await pipeline.FireEvent(@event);
            actWrapper.HasBeenCalled.Should().BeTrue();
        }

        [TestMethod]
        public async Task Pipeline_ShouldPopulateTheContextWithTheDispatcher()
        {
            var @event = new Event();
            await pipeline.FireEvent(@event);

            actWrapper.PipelineContext.Dispathcer.ShouldBeEquivalentTo(dispatcherMock.Object);
        }

        [TestMethod]
        public async Task Pipeline_ShouldPopulateTheReconciliationServiceField()
        {
            var @event = new Event();
            await pipeline.FireEvent(@event);

            actWrapper.PipelineContext.ReconciliationService.ShouldBeEquivalentTo(reconciliationServiceMock.Object);
        }

        [TestMethod]
        public async Task Pipeline_ShouldPopulateTheContextWithTheEventStoreField()
        {
            var @event = new Event();
            await pipeline.FireEvent(@event);

            actWrapper.PipelineContext.EventStore.ShouldBeEquivalentTo(eventStoreMock.Object);
        }
    }
}
