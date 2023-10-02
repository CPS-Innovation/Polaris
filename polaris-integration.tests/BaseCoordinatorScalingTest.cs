using polaris_gateway.integration.tests.Proxies;

namespace polaris_integration.tests
{
    public class BaseCoordinatorScalingTest : CoordinatorApiProxy
    {
        protected static int _testCases;

        protected string urn = null;
        protected int caseId;

        static BaseCoordinatorScalingTest()
        {
            _testCases = int.Parse(_config["ScalingTest:TestCases"]);
        }

        public BaseCoordinatorScalingTest(int instance)
        {
            if (instance <= _testCases)
            {
                urn = $"TEST100-0{instance.ToString("00")}";
                caseId = 100 + instance;
            }
        }

        public async Task BaseDeleteAndRefreshCase()
        {
            if(urn != null)
            {
                await CaseDeleteAsync(urn, caseId);
                await CaseRefreshAsync(urn, caseId, Guid.NewGuid().ToString());
            }
        }

        /*protected async Task RefreshCaseAsync(string urn, int caseId)
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();

            try 
            {
                // Act
                await CaseRefreshAsync(urn, caseId, correlationId);
                TrackerDto tracker = await WaitForCompletedTrackerAsync(urn, caseId, correlationId);

                // Assert
                tracker
                    .Documents
                    .Where(d => d.CmsDocType.DocumentType != "DAC" && d.CmsDocType.DocumentType != "PCD")   // Currently removed from coordinator
                    .Select(t => t.Status)
                    .ToList()
                    .Should().AllBeEquivalentTo(DocumentStatus.Indexed);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }   
       }*/
    }
}