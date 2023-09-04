using Common.Dto.Tracker;
using FluentAssertions;
using polaris_gateway.integration.tests.Proxies;

namespace polaris_integration.tests
{
    public class BaseCoordinatorScalingTest : CoordinatorApiProxy
    {
        protected async Task RefreshCaseAsync(string urn, int caseId)
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
       }
    }
}