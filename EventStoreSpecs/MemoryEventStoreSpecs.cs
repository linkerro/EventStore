using EventStore;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventStoreSpecs
{
    public class MemoryEventStoreSpecs
    {
        [TestClass]
        public class SaveSpecs
        {
            [TestMethod]
            public async Task ShouldSaveEventsAsync()
            {
                var eventStore = new MemoryEventStore();
                var eventToBeSaved = new TestEvent { Property1 = "test" };

                var storedEvent = await eventStore.Save(eventToBeSaved);
                Assert.IsNotNull(storedEvent);
            }

            [TestMethod]
            public async Task ShouldPopulateTheIdFieldAsync()
            {
                var eventStore = new MemoryEventStore();
                var eventToBeSaved = new TestEvent { Property1 = "test" };

                var storedEvent = await eventStore.Save(eventToBeSaved);

                Assert.AreNotEqual(Guid.Empty, storedEvent.Id);
            }

            [TestMethod]
            public async Task ShouldReturnNewEventInstanceAsync()
            {
                var eventStore = new MemoryEventStore();
                var eventToBeSaved = new TestEvent { Property1 = "test" };

                var storedEvent = await eventStore.Save(eventToBeSaved);

                Assert.AreNotSame(eventToBeSaved, storedEvent);
            }

            [TestMethod]
            public async Task ShouldNotAffectOriginalInstanceDuringSaveAsync()
            {
                var eventStore = new MemoryEventStore();
                var eventToBeSaved = new TestEvent { Property1 = "test" };
                var originalEventSerialized = JsonConvert.SerializeObject(eventToBeSaved);

                var storedEvent = await eventStore.Save(eventToBeSaved);

                Assert.AreEqual(originalEventSerialized, JsonConvert.SerializeObject(eventToBeSaved));
            }
        }

        [TestClass]
        public class GenericBasedGetByIdSpecs
        {
            [TestMethod]
            public async Task ShouldGetByIdAsync()
            {
                var eventStore = new MemoryEventStore();
                var eventToBeSaved = new TestEvent { Property1 = "test" };

                var storedEvent = await eventStore.Save(eventToBeSaved);
                var retreivedEvent = await eventStore.Get<TestEvent>(storedEvent.Id);

                Assert.AreEqual(JsonConvert.SerializeObject(storedEvent), JsonConvert.SerializeObject(retreivedEvent));
            }
        }

        [TestClass]
        public class GetByMultipleIdsSpecs
        {
            [TestMethod]
            public async Task ShouldGetByIdsAsync()
            {
                var eventStore = new MemoryEventStore();
                var eventToBeSaved1 = new TestEvent { Property1 = "test" };
                var eventToBeSaved2 = new TestEvent2 { Property1 = "property1", Property2 = "property2" };

                var storedEvent1 = await eventStore.Save(eventToBeSaved1);
                var storedEvent2 = await eventStore.Save(eventToBeSaved2);

                var expectedEvents = new List<Event> { storedEvent1, storedEvent2 };
                var retreivedEvents = await eventStore.Get(new List<Guid> { storedEvent1.Id, storedEvent2.Id });

                Assert.AreEqual(JsonConvert.SerializeObject(expectedEvents), JsonConvert.SerializeObject(retreivedEvents));
            }
        }

        [TestClass]
        public class IndexSpecs
        {
            [TestMethod]
            public void ShouldInxedEventAsync()
            {
                var savedEvent = new TestEvent { Property1 = "test", Id = new Guid() };
                var eventStore = new MemoryEventStore();

                Action action = async () => await eventStore.Index(savedEvent, "testIndex", new { Property1 = "" });
                action.ShouldNotThrow();
            }
        }

        [TestClass]
        public class GetByIndex
        {
            [TestMethod]
            public async Task ShouldRetrieveEventFromIndexAsync()
            {
                var unsavedEvent1 = new TestEvent { Property1 = "test" };
                var unsavedEvent2 = new TestEvent2 { Property1 = "test", Property2 = "test2" };
                var eventStore = new MemoryEventStore();

                var savedEvent1 = await eventStore.Save(unsavedEvent1);
                var savedEvent2 = await eventStore.Save(unsavedEvent2);

                object indexKey = new { Property1 = "" };
                var indexName = "indexName";
                await eventStore.Index(savedEvent1, indexName, indexKey);
                await eventStore.Index(savedEvent2, indexName, indexKey);

                var indexedEvents = await eventStore.GetByIndex(indexName);
                Assert.AreEqual(2, indexedEvents.Count());
            }
        }

        [TestClass]
        public class GetFromIndex
        {
            [TestMethod]
            public async Task ShouldRetrieveEventFromIndexAsync()
            {
                var unsavedEvent1 = new TestEvent { Property1 = "test" };
                var unsavedEvent2 = new TestEvent2 { Property1 = "test", Property2 = "test2" };
                var eventStore = new MemoryEventStore();

                var savedEvent1 = await eventStore.Save(unsavedEvent1);
                var savedEvent2 = await eventStore.Save(unsavedEvent2);

                object indexKey = new { Property1 = "" };
                var indexName = "indexName";
                await eventStore.Index(savedEvent1, indexName, indexKey);
                await eventStore.Index(savedEvent2, indexName, indexKey);

                var indexedEvents = await eventStore.GetFromIndex(indexName,new { Property1 = "test" });
                Assert.AreEqual(2, indexedEvents.Count());
            }
        }
    }

    public class TestEvent : Event
    {
        public Guid Id { get; set; }
        public string Property1 { get; set; }
    }

    public class TestEvent2 : Event
    {
        public Guid Id { get; set; }
        public string Property1 { get; set; }
        public string Property2 { get; set; }
    }

    public struct TestEventIndexKey
    {
        public string Property1 { get; set; }
    }
}
