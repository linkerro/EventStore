using EventStore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventStoreSpecs
{
    public class MemoryEventStoreSpecs
    {
        [TestClass]
        public class SaveSpecs
        {
            [TestMethod]
            public void ShouldSaveEvents()
            {
                var eventStore = new MemoryEventStore();
                var eventToBeSaved = new TestEvent { Property1 = "test" };

                var storedEvent = eventStore.Save(eventToBeSaved);
                Assert.IsNotNull(storedEvent);
            }

            [TestMethod]
            public void ShouldPopulateTheIdField()
            {
                var eventStore = new MemoryEventStore();
                var eventToBeSaved = new TestEvent { Property1 = "test" };

                var storedEvent = eventStore.Save(eventToBeSaved);

                Assert.AreNotEqual(Guid.Empty, storedEvent.Id);
            }

            [TestMethod]
            public void ShouldReturnNewEventInstance()
            {
                var eventStore = new MemoryEventStore();
                var eventToBeSaved = new TestEvent { Property1 = "test" };

                var storedEvent = eventStore.Save(eventToBeSaved);

                Assert.AreNotSame(eventToBeSaved, storedEvent);
            }

            [TestMethod]
            public void ShouldNotAffectOriginalInstanceDuringSave()
            {
                var eventStore = new MemoryEventStore();
                var eventToBeSaved = new TestEvent { Property1 = "test" };
                var originalEventSerialized = JsonConvert.SerializeObject(eventToBeSaved);

                var storedEvent = eventStore.Save(eventToBeSaved);

                Assert.AreEqual(originalEventSerialized, JsonConvert.SerializeObject(eventToBeSaved));
            }
        }

        [TestClass]
        public class GenericBasedGetByIdSpecs
        {
            [TestMethod]
            public void ShouldGetById()
            {
                var eventStore = new MemoryEventStore();
                var eventToBeSaved = new TestEvent { Property1 = "test" };

                var storedEvent = eventStore.Save(eventToBeSaved);
                var retreivedEvent = eventStore.Get<TestEvent>(storedEvent.Id);

                Assert.AreEqual(JsonConvert.SerializeObject(storedEvent), JsonConvert.SerializeObject(retreivedEvent));
            }
        }

        [TestClass]
        public class GetByMultipleIdsSpecs
        {
            [TestMethod]
            public void ShouldGetByIds()
            {
                var eventStore = new MemoryEventStore();
                var eventToBeSaved1 = new TestEvent { Property1 = "test" };
                var eventToBeSaved2 = new TestEvent2 { Property1 = "property1", Property2 = "property2" };

                var storedEvent1 = eventStore.Save(eventToBeSaved1);
                var storedEvent2 = eventStore.Save(eventToBeSaved2);

                var expectedEvents = new List<Event> { storedEvent1, storedEvent2 };
                var retreivedEvents = eventStore.Get(new List<Guid> { storedEvent1.Id, storedEvent2.Id });

                Assert.AreEqual(JsonConvert.SerializeObject(expectedEvents), JsonConvert.SerializeObject(retreivedEvents));
            }
        }

        [TestClass]
        public class IndexSpecs
        {
            [TestMethod]
            public void ShouldInxedEvent()
            {
                var savedEvent = new TestEvent { Property1 = "test", Id = new Guid() };
                var eventStore = new MemoryEventStore();

                eventStore.Index<TestEventIndexKey>(savedEvent);
            }
        }

        [TestClass]
        public class GetByIndexSingleKey
        {
            [TestMethod]
            public void ShouldRetrieveEventFromIndex()
            {
                var unsavedEvent1 = new TestEvent { Property1 = "test" };
                var unsavedEvent2 = new TestEvent2 { Property1 = "test", Property2 ="test2"};
                var eventStore = new MemoryEventStore();

                var savedEvent1 = eventStore.Save(unsavedEvent1);
                var savedEvent2 = eventStore.Save(unsavedEvent2);

                eventStore.Index<TestEventIndexKey>(savedEvent1);
                eventStore.Index<TestEventIndexKey>(savedEvent2);

                var indexedEvents = eventStore.GetByIndex(new TestEventIndexKey { Property1 = "test" });
                Assert.AreEqual(2, indexedEvents.Count());
            }
        }

        [TestClass]
        public class GetByIndexMultipleKeys
        {
            [TestMethod]
            public void ShouldRetrieveEventsFromIndex()
            {
                var unsavedEvent1 = new TestEvent { Property1 = "test" };
                var unsavedEvent2 = new TestEvent2 { Property1 = "test2", Property2 = "test2" };
                var eventStore = new MemoryEventStore();

                var savedEvent1 = eventStore.Save(unsavedEvent1);
                var savedEvent2 = eventStore.Save(unsavedEvent2);

                eventStore.Index<TestEventIndexKey>(savedEvent1);
                eventStore.Index<TestEventIndexKey>(savedEvent2);

                var indexedEvents = eventStore.GetByIndexKeys(new List<TestEventIndexKey> {
                    new TestEventIndexKey{Property1="test"},
                    new TestEventIndexKey{Property1="test2"}
                });
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
