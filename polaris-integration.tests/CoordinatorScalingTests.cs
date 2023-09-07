#if SCALING_TESTS
using polaris_integration.tests;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace polaris_integration.coordinator.tests
{
    // Test 1, 2, 3
    public class AlphabeticalOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(
            IEnumerable<TTestCase> testCases) where TTestCase : ITestCase =>
            testCases.OrderBy(testCase => testCase.TestMethod.Method.Name);
    }

    [TestCaseOrderer(ordererTypeName: "polaris_integration.coordinator.tests.AlphabeticalOrderer", ordererAssemblyName: "polaris-integration.tests")]
    public class CoordinatorScalingTests : BaseCoordinatorScalingTest
    {
        const int _testCases = 10;

        [Fact]
        public async Task Test_1_Delete_Cases()
        {
            List<Task> tasks = new List<Task>();

            for (var i = 1; i <= _testCases; i++)
            {
                var urn = $"TEST100-0{i.ToString("00")}";
                var caseId = 100 + i;

                tasks.Add(CaseDeleteAsync(urn, caseId));
            }

            await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task Test_2_Refresh_Cases()
        {
            List<Task> tasks = new List<Task>();

            for (var i = 1; i <= _testCases; i++)
            {
                var urn = $"TEST100-0{i.ToString("00")}";
                var caseId = 100 + i;

                tasks.Add(CaseRefreshAsync(urn, caseId, Guid.NewGuid().ToString()) );
            }

            await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task Test_3_WaitFor_Trackers()
        {
            List<Task> tasks = new List<Task>();

            for (var i = 1; i <= _testCases; i++)
            {
                var urn = $"TEST100-0{i.ToString("00")}";
                var caseId = 100 + i;

                tasks.Add(WaitForCompletedTrackerAsync(urn, caseId, Guid.NewGuid().ToString()));
            }

            await Task.WhenAll(tasks);
        }
    }
}
#endif