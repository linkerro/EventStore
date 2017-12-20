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
        private ActAsync nopActAsync = (e, context) => Task.CompletedTask;
        private Mock<IDispatcher> dispatcherMock;
        private Mock<IReconciliationService> reconciliationServiceMock;
        private Mock<IEventStore> eventStoreMock;
        private ActWrapper actWrapper;
        private ActWrapper actWrapperAsync;
        private Pipeline pipeline;
        private Pipeline asyncPipeline;

        [TestInitialize]
        public void Initialize()
        {
            dispatcherMock = new Mock<IDispatcher>(MockBehavior.Strict);
            reconciliationServiceMock = new Mock<IReconciliationService>(MockBehavior.Strict);
            eventStoreMock = new Mock<IEventStore>(MockBehavior.Strict);
            actWrapper = ActWrapper.From(nopAct);
            actWrapperAsync = ActWrapper.From(nopActAsync);
            pipeline = new Pipeline(
                new List<Type> { typeof(Event) },
                new List<Actor> { actWrapper.Act },
                dispatcherMock.Object,
                reconciliationServiceMock.Object,
                eventStoreMock.Object
                );
            asyncPipeline = new Pipeline(
                 new List<Type> { typeof(Event) },
                 new List<Actor> { actWrapperAsync.ActAsync },
                 dispatcherMock.Object,
                 reconciliationServiceMock.Object,
                 eventStoreMock.Object
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
        public void Pipeline_ShouldFireEventsToAsyncActs()
        {
            var @event = new Event();
            asyncPipeline.FireEvent(@event);
            actWrapperAsync.HasBeenCalled.Should().BeTrue();
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

        [TestMethod]
        public void Pipeline_ShouldPopulateTheContextWithTheEventStoreField()
        {
            var @event = new Event();
            pipeline.FireEvent(@event);

            actWrapper.PipelineContext.EventStore.ShouldBeEquivalentTo(eventStoreMock.Object);
        }
    }
}
