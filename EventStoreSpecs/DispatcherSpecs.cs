﻿using EventStore;
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
        private ReconciliationService reconciliationService;
        private Dispatcher dispatcher;
        private Act<Event> nopAct = (e, context) => { };

        [TestInitialize]
        public void TestInitialization()
        {
            eventStore = new MemoryEventStore();
            reconciliationService = new ReconciliationService();

            dispatcher = new Dispatcher(eventStore, reconciliationService);
        }

        [TestMethod]
        public void DispatcherShouldRegisterPipeline()
        {
            var eventTypes = new List<Type>
            {
                typeof(Event1)
            };
            var actions = new ActList { nopAct };
            Action action = () => dispatcher.RegisterPipeline(eventTypes, actions);
            action.ShouldNotThrow();
        }

        [TestMethod]
        public async Task DispatcherShouldDispatchEventToPipelineAsync()
        {
            var actWrapper = ActWrapper<Event>.From(nopAct);
            var actions = new ActList { actWrapper.Act };
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
            var actWrapperForPipe1 = ActWrapper<Event>.From(nopAct);
            var actWrapperForPipe2 = ActWrapper<Event>.From(nopAct);

            var eventTypes = new List<Type> { typeof(Event1) };
            var actsForPipe1 = new ActList { actWrapperForPipe1.Act };
            var actsForPipe2 = new ActList { actWrapperForPipe2.Act };
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
            var actWrapper = ActWrapper<Event>.From(nopAct);

            var eventTypes = new List<Type> { typeof(Event1), typeof(Event2) };
            var acts = new ActList { actWrapper.Act };
            dispatcher.RegisterPipeline(eventTypes, acts);

            Event1 event1 = new Event1 { Property = "event1" };
            dispatcher.Dispatch(event1);
            await actWrapper.ActTask;

            (actWrapper.Event as Event1).Property.ShouldBeEquivalentTo(event1.Property);

            actWrapper.Rearm();
            Event2 event2 = new Event2 { Property = "event2" };
            dispatcher.Dispatch(event2);
            await actWrapper.ActTask;

            (actWrapper.Event as Event2).Property.ShouldBeEquivalentTo(event2.Property);
        }

        [TestMethod]
        public async Task DispatcherShouldSaveEventAsync()
        {
            var actWrapper = ActWrapper<Event>.From(nopAct);

            var eventTypes = new List<Type> { typeof(Event1) };
            var acts = new ActList { actWrapper.Act };
            dispatcher.RegisterPipeline(eventTypes, acts);

            var originalEvent = new Event1();
            dispatcher.Dispatch(originalEvent);
            await actWrapper.ActTask;

            var savedEvent = await eventStore.Get<Event1>(actWrapper.Event.Id);
            savedEvent.Should().NotBeNull();
        }

        [TestMethod]
        public async Task DispatcherShoulDispatchWithReconciliation()
        {
            var resolvedEvent = new ReconciliationEvent { Id = Guid.NewGuid() };
            var actWrapper = ActWrapper<Event>
                .From((e, context) =>
                {
                    resolvedEvent.ReconciliationId = (e as ReconciliationEvent).ReconciliationId;
                    context.ReconciliationService.ResolveTask(resolvedEvent);
                });

            var eventTypes = new List<Type> { typeof(TestReconciliationEvent) };
            var acts = new ActList { actWrapper.Act };
            dispatcher.RegisterPipeline(eventTypes, acts);

            var originalEvent = new TestReconciliationEvent();
            var reconciliationEvent = await dispatcher.DispatchWithReconciliation(originalEvent);
            reconciliationEvent.ShouldBeEquivalentTo(resolvedEvent);
        }

        class Event1 : Event
        {
            public string Property { get; set; }
        }

        class Event2 : Event
        {
            public string Property { get; set; }
        }

        class TestReconciliationEvent : ReconciliationEvent
        {

        }
    }
}
