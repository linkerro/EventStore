using EventStore;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace EventStoreSpecs
{
    [TestClass]
    public class ReconciliationServiceTests
    {
        private ReconciliationService reconciliationService;

        [TestInitialize]
        public void TestInitialize()
        {
            reconciliationService = new ReconciliationService();
        }

        [TestMethod]
        public void ReconciliationService_ShouldReturnNewReconciliationTask()
        {
            Guid reconciliationId = Guid.NewGuid();
            var reconciliationTask= reconciliationService.GetReconciliationTask(reconciliationId);
            reconciliationTask.Should().NotBeNull();
        }

        [TestMethod]
        public async Task ReconciliationService_ShouldResolveAnOutstandingTaskAsync()
        {
            Guid reconciliationId = Guid.NewGuid();
            var reconciliationTask = reconciliationService.GetReconciliationTask(reconciliationId);
            var reconciliationEvent = new ReconciliationTestEvent { ReconciliationId = reconciliationId };
            reconciliationService.ResolveTask(reconciliationEvent);
            var result = await reconciliationTask;
            result.ShouldBeEquivalentTo(reconciliationEvent);
        }

        private class ReconciliationTestEvent : ReconciliationEvent
        {
        }
    }
}
