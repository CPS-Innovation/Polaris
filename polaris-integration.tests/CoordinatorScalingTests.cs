#if INTEGRATION_TESTS
using polaris_integration.tests;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace polaris_integration.coordinator.tests
{
    // Tests 01, 02, 03... 10 are run in parallel
    public class AlphabeticalOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(
            IEnumerable<TTestCase> testCases) where TTestCase : ITestCase =>
            testCases.OrderBy(testCase => testCase.TestMethod.Method.Name);
    }

    [TestCaseOrderer(ordererTypeName: "polaris_integration.coordinator.tests.AlphabeticalOrderer", ordererAssemblyName: "polaris-integration.tests")]
    public class CoordinatorScalingTest_01 : BaseCoordinatorScalingTest
    {
        public CoordinatorScalingTest_01() : base(1)
        {}

        [Fact]
        public async Task DeleteAndRefreshCase()
        {
            await BaseDeleteAndRefreshCase();
        }

        [Fact]
        public async Task WaitForCompletedTracker_1()
        {
            await WaitForCompletedTrackerAsync(urn, caseId, Guid.NewGuid().ToString());
        }
    }

    [TestCaseOrderer(ordererTypeName: "polaris_integration.coordinator.tests.AlphabeticalOrderer", ordererAssemblyName: "polaris-integration.tests")]
    public class CoordinatorScalingTest_02 : BaseCoordinatorScalingTest
    {
        public CoordinatorScalingTest_02() : base(2)
        { }

        [Fact]
        public async Task DeleteAndRefreshCase()
        {
            await BaseDeleteAndRefreshCase();
        }

        [Fact]
        public async Task WaitForCompletedTracker_2()
        {
            if (_testCases >= 2)
                await WaitForCompletedTrackerAsync(urn, caseId, Guid.NewGuid().ToString());
        }
    }

    [TestCaseOrderer(ordererTypeName: "polaris_integration.coordinator.tests.AlphabeticalOrderer", ordererAssemblyName: "polaris-integration.tests")]
    public class CoordinatorScalingTest_03 : BaseCoordinatorScalingTest
    {
        public CoordinatorScalingTest_03() : base(3)
        { }

        [Fact]
        public async Task DeleteAndRefreshCase()
        {
            await BaseDeleteAndRefreshCase();
        }

        [Fact]
        public async Task WaitForCompletedTracker_3()
        {
            if (_testCases >= 3)
                await WaitForCompletedTrackerAsync(urn, caseId, Guid.NewGuid().ToString());
        }
    }

    [TestCaseOrderer(ordererTypeName: "polaris_integration.coordinator.tests.AlphabeticalOrderer", ordererAssemblyName: "polaris-integration.tests")]
    public class CoordinatorScalingTest_04 : BaseCoordinatorScalingTest
    {
        public CoordinatorScalingTest_04() : base(4)
        { }

        [Fact]
        public async Task DeleteAndRefreshCase()
        {
            await BaseDeleteAndRefreshCase();
        }

        [Fact]
        public async Task WaitForCompletedTracker_4()
        {
            if (_testCases >= 4)
                await WaitForCompletedTrackerAsync(urn, caseId, Guid.NewGuid().ToString());
        }
    }

    [TestCaseOrderer(ordererTypeName: "polaris_integration.coordinator.tests.AlphabeticalOrderer", ordererAssemblyName: "polaris-integration.tests")]
    public class CoordinatorScalingTest_05 : BaseCoordinatorScalingTest
    {
        public CoordinatorScalingTest_05() : base(5)
        { }

        [Fact]
        public async Task DeleteAndRefreshCase()
        {
            await BaseDeleteAndRefreshCase();
        }

        [Fact]
        public async Task WaitForCompletedTracker_5()
        {
            if (_testCases >= 5)
                await WaitForCompletedTrackerAsync(urn, caseId, Guid.NewGuid().ToString());
        }
    }

    [TestCaseOrderer(ordererTypeName: "polaris_integration.coordinator.tests.AlphabeticalOrderer", ordererAssemblyName: "polaris-integration.tests")]
    public class CoordinatorScalingTest_06 : BaseCoordinatorScalingTest
    {
        public CoordinatorScalingTest_06() : base(6)
        { }

        [Fact]
        public async Task DeleteAndRefreshCase()
        {
            await BaseDeleteAndRefreshCase();
        }

        [Fact]
        public async Task WaitForCompletedTracker_6()
        {
            if (_testCases >= 6)
                await WaitForCompletedTrackerAsync(urn, caseId, Guid.NewGuid().ToString());
        }
    }

    [TestCaseOrderer(ordererTypeName: "polaris_integration.coordinator.tests.AlphabeticalOrderer", ordererAssemblyName: "polaris-integration.tests")]
    public class CoordinatorScalingTest_07 : BaseCoordinatorScalingTest
    {
        public CoordinatorScalingTest_07() : base(7)
        { }

        [Fact]
        public async Task DeleteAndRefreshCase()
        {
            await BaseDeleteAndRefreshCase();
        }

        [Fact]
        public async Task WaitForCompletedTracker_7()
        {
            if (_testCases >= 7)
                await WaitForCompletedTrackerAsync(urn, caseId, Guid.NewGuid().ToString());
        }
    }

    [TestCaseOrderer(ordererTypeName: "polaris_integration.coordinator.tests.AlphabeticalOrderer", ordererAssemblyName: "polaris-integration.tests")]
    public class CoordinatorScalingTest_08 : BaseCoordinatorScalingTest
    {
        public CoordinatorScalingTest_08() : base(8)
        { }

        [Fact]
        public async Task DeleteAndRefreshCase()
        {
            await BaseDeleteAndRefreshCase();
        }

        [Fact]
        public async Task WaitForCompletedTracker_8()
        {
            if (_testCases >= 8)
                await WaitForCompletedTrackerAsync(urn, caseId, Guid.NewGuid().ToString());
        }
    }

    [TestCaseOrderer(ordererTypeName: "polaris_integration.coordinator.tests.AlphabeticalOrderer", ordererAssemblyName: "polaris-integration.tests")]
    public class CoordinatorScalingTest_09 : BaseCoordinatorScalingTest
    {
        public CoordinatorScalingTest_09() : base(9)
        { }

        [Fact]
        public async Task DeleteAndRefreshCase()
        {
            await BaseDeleteAndRefreshCase();
        }

        [Fact]
        public async Task WaitForCompletedTracker_9()
        {
            if (_testCases >= 9)
                await WaitForCompletedTrackerAsync(urn, caseId, Guid.NewGuid().ToString());
        }
    }

    [TestCaseOrderer(ordererTypeName: "polaris_integration.coordinator.tests.AlphabeticalOrderer", ordererAssemblyName: "polaris-integration.tests")]
    public class CoordinatorScalingTest_10 : BaseCoordinatorScalingTest
    {
        public CoordinatorScalingTest_10() : base(10)
        { }

        [Fact]
        public async Task DeleteAndRefreshCase()
        {
            await BaseDeleteAndRefreshCase();
        }

        [Fact]
        public async Task WaitForCompletedTracker_10()
        {
            if (_testCases >= 10)
                await WaitForCompletedTrackerAsync(urn, caseId, Guid.NewGuid().ToString());
        }
    }
}
#endif