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
        private Act<Event> nopAct = (e, context) => { };
        private Mock<IDispatcher> dispatcherMock;
        private Mock<IReconciliationService> reconciliationServiceMock;
        private Mock<IEventStore> eventStoreMock;
        private ActWrapper<Event> actWrapper;
        private Pipeline pipeline;

        [TestInitialize]
        public void Initialize()
        {
            dispatcherMock = new Mock<IDispatcher>(MockBehavior.Strict);
            reconciliationServiceMock = new Mock<IReconciliationService>(MockBehavior.Strict);
            eventStoreMock = new Mock<IEventStore>(MockBehavior.Strict);
            actWrapper = ActWrapper<Event>.From(nopAct);
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

        [TestMethod]
        public async Task Pipeline_ShouldHandleExceptions()
        {
            var @event = new Event();
            pipeline = new Pipeline(
                new Type[] { typeof(Event) },
                new ActList
                {
                    (Event actEvent,PipelineContext context)=>
                    {
                        throw new Exception();
                    }
                },
                dispatcherMock.Object,
                reconciliationServiceMock.Object,
                eventStoreMock.Object);

            dispatcherMock
                .Setup(dispatcher => dispatcher.Dispatch(It.IsAny<ExceptionEvent>()));

            await pipeline.FireEvent(@event);
        }

        [TestMethod]
        public async Task Pipeline_ShouldReconcileEventsWithException()
        {
            Guid reconciliationId = Guid.NewGuid();
            var @event = new ReconciliationEvent {
                ReconciliationId = reconciliationId
            };
            pipeline = new Pipeline(
                new Type[] { typeof(Event) },
                new ActList
                {
                    (Event actEvent,PipelineContext context)=>
                    {
                        throw new Exception();
                    }
                },
                dispatcherMock.Object,
                reconciliationServiceMock.Object,
                eventStoreMock.Object);

            dispatcherMock
                .Setup(dispatcher => dispatcher.Dispatch(It.IsAny<ExceptionEvent>()));
            ExceptionEvent thrownExceptionEvent=null;
            reconciliationServiceMock
                .Setup(service => service.ResolveTask(It.IsAny<ExceptionEvent>()))
                .Callback<Event>((receivedEvent) => thrownExceptionEvent = (ExceptionEvent)receivedEvent);
            await pipeline.FireEvent(@event);
            thrownExceptionEvent.ReconciliationId.Should().Be(reconciliationId);
        }
    }
}
