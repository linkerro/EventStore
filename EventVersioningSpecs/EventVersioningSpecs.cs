using EventStore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventVersioningSpecs
{
    [TestClass]
    public class EventVersioningSpecs
    {
        [TestMethod]
        public void ShouldMapOlderEventFormatsToNewEventTypes()
        {
            var eventWithVersion1 = new VersionedEvent1 { Property1 = "property1" };
            var eventWithVersion2 = new VersionedEvent2 { Property1 = "property1", Property2 = "property2" };
            var eventWithVersion3 = new VersionedEvent { Property3 = "test" };

            var eventStore = new MemoryEventStore();
            var savedEvent1 = eventStore.Save(eventWithVersion1);
            var savedEvent2 = eventStore.Save(eventWithVersion2);
            var savedEvent3 = eventStore.Save(eventWithVersion3);

            var ids = new List<Guid> { savedEvent1.Id, savedEvent2.Id, savedEvent3.Id };
            var events = eventStore.Get(ids);
            var eventTypeCount = events
                .Select(e => e.GetType())
                .Where(e => e.Name == "VersionedEvent")
                .Count();

            Assert.AreEqual(3, eventTypeCount);
        }
    }

    [EventVersionTransforms(typeof(VersionedEventTransforms))]
    public class VersionedEvent : Event
    {
        public VersionedEvent()
        {
            Version = 3;
        }
        public string Property3 { get; set; }
    }

    public class VersionedEvent2 : Event
    {
        public VersionedEvent2()
        {
            Version = 2;
        }
        public new string Name { get { return "VersionedEvent"; } }
        public string Property1 { get; set; }
        public string Property2 { get; set; }
    }

    public class VersionedEvent1 : Event
    {
        public VersionedEvent1()
        {
            Version = 1;
        }
        public string Property1 { get; set; }
        public new string Name { get { return "VersionedEvent"; } }
    }

    public class VersionedEventTransforms : TransformInfo
    {
        public VersionedEventTransforms()
        {
            Transforms = new Dictionary<int, Materializer.Transform>
            {
                { 1, From1to2 },
                { 2, From2To3 }
            };
        }

        JObject From1to2(JObject @event)
        {
            @event["Property2"] = "default property 2";
            return @event;
        }

        JObject From2To3(JObject @event)
        {
            @event["Property3"] = $"{@event["Property1"]} {@event["Property2"]}";
            return @event;
        }
    }
}
