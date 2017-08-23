using EventStore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace EventStoreSpecs
{
    [TestClass]
    public class EventSpecs
    {
        [TestMethod]
        public void ShouldCopyUsingGenerics()
        {
            var @event = new TestEvent();
            var eventCopy = @event.Copy<TestEvent>();
            Assert.AreEqual(JsonConvert.SerializeObject(@event), JsonConvert.SerializeObject(eventCopy));
        }

        [TestMethod]
        public void ShouldCopyUsingType()
        {
            var @event = new TestEvent();
            var eventCopy = @event.Copy() as TestEvent;
            Assert.AreEqual(JsonConvert.SerializeObject(@event), JsonConvert.SerializeObject(eventCopy));
        }
    }
}
