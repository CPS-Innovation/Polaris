#if INTEGRATION_TESTS
using Common.Dto.Request;
using Common.Dto.Request.Redaction;
using Common.Dto.Tracker;
using FluentAssertions;
using polaris_gateway.integration.tests.Proxies;
using System.Net;

namespace polaris_integration.tests
{
    public class GatewayTests : GatewayApiProxy
    {
        readonly string e2e0 = "E2E00000-0000-0000-0000-000000000000";
        readonly string e2e1 = "E2E00000-0000-0000-0000-000000000001";
        readonly string e2e2 = "E2E00000-0000-0000-0000-000000000002";
        readonly string e2e3 = "E2E00000-0000-0000-0000-000000000003";

        public GatewayTests()
        {
        }

        [Fact]
        public async Task Find_CaseByUrn_IsSuccessful()
        {
            // Arrange
            var urn = "42MZ7238121";
            var caseId = 2146765;

            // Act
            var cases = await GetCasesAsync(urn);
            var @case = await GetCaseAsync(urn, caseId);


            // Assert
            cases.Count().Should().Be(1);
            cases.First().Id.Should().Be(caseId);   
            cases.First().UniqueReferenceNumber.Should().Be(urn);

            @case.Id.Should().Be(caseId);
            @case.UniqueReferenceNumber.Should().Be(urn);
        }

        [Fact]
        public async Task RefreshAndRedact_Case_IsSuccessful()
        {
            // Arrange
            var urn = "01VK0000421";
            var caseId = 2146928;
            var document1Id = "CMS-8662332";
            var document2Id = "CMS-8662333";

            var searchExpectations = new (string query, bool exists)[]
              {
                  ("aspose", false),    // special check that Aspose license has not expired (will appear in watermark)
                  ("one", true),
                  ("two", true),
                  ("three", true),
                  ("four", false),
                  ("alice", true),
                  ("bob", true),
                  ("carol", true),
                  ("dave", false)
              };

            // Act / Assert

            #region Initial Refresh
            await CaseDelete(urn, caseId);

            var status = await CaseRefresh(urn, caseId, e2e1);
            status.Should().Be(HttpStatusCode.Accepted);

            TrackerDto tracker = await WaitForCompletedTracker(urn, caseId, e2e0);

            tracker.Documents.Select(t => t.Status).ToList().Should().AllBeEquivalentTo(DocumentStatus.Indexed);
            tracker.Documents.Select(t => t.CmsDocType.DocumentType == "MG 5").ToList().Should().NotBeEmpty();
            tracker.Documents.Select(t => t.CmsDocType.DocumentType == "PCD").ToList().Should().NotBeEmpty();
            tracker.Documents.Select(t => t.CmsDocType.DocumentType == "DAC").ToList().Should().NotBeEmpty();

            await CheckSearchAssertions(urn, caseId, e2e0, searchExpectations);
            #endregion

            #region 1st Redaction (Phase 2)
            await DocumentCheckout(urn, caseId, document1Id, e2e2);

            DocumentRedactionSaveRequestDto redaction = new DocumentRedactionSaveRequestDto
            {
                DocId = document1Id,
                Redactions = new List<RedactionDefinitionDto>
                {
                    new RedactionDefinitionDto
                    {
                        PageIndex = 1,
                        Height = 1,
                        Width = 1,
                        RedactionCoordinates = new List<RedactionCoordinatesDto>
                        {
                            new RedactionCoordinatesDto
                            {
                                X1 = 0,
                                Y1 = 0,
                                X2 = 1,
                                Y2 = 1
                            }
                        }
                    }
                }
            };

            await RedactDocument(urn, caseId, document1Id, redaction, e2e2);

            status = await CaseRefresh(urn, caseId, e2e2);
            status.Should().Be(HttpStatusCode.Accepted);

            tracker = await WaitForCompletedTracker(urn, caseId, e2e2);

            tracker.Documents.Select(t => t.Status).ToList().Should().AllBeEquivalentTo(DocumentStatus.Indexed);
            tracker.Documents.Select(t => t.CmsDocType.DocumentType == "MG 5").ToList().Should().NotBeEmpty();
            tracker.Documents.Select(t => t.CmsDocType.DocumentType == "PCD").ToList().Should().NotBeEmpty();
            tracker.Documents.Select(t => t.CmsDocType.DocumentType == "DAC").ToList().Should().NotBeEmpty();

            searchExpectations = new (string query, bool exists)[]
              {
                  ("one", true),
                  ("two", true),
                  ("three", false),
                  ("four", true),
                  ("alice", true),
                  ("bob", true),
                  ("carol", true),
                  ("dave", false)
              };

            await CheckSearchAssertions(urn, caseId, e2e0, searchExpectations);
            #endregion

            #region 2nd Redaction (Phase 3)
            await DocumentCheckout(urn, caseId, document2Id, e2e3);

            redaction = new DocumentRedactionSaveRequestDto
            {
                DocId = document1Id,
                Redactions = new List<RedactionDefinitionDto>
                {
                    new RedactionDefinitionDto
                    {
                        PageIndex = 1,
                        Height = 1,
                        Width = 1,
                        RedactionCoordinates = new List<RedactionCoordinatesDto>
                        {
                            new RedactionCoordinatesDto
                            {
                                X1 = 0,
                                Y1 = 0,
                                X2 = 1,
                                Y2 = 1
                            }
                        }
                    }
                }
            };

            await RedactDocument(urn, caseId, document2Id, redaction, e2e3);

            status = await CaseRefresh(urn, caseId, e2e3);
            status.Should().Be(HttpStatusCode.Accepted);

            tracker = await WaitForCompletedTracker(urn, caseId, e2e3);

            tracker.Documents.Select(t => t.Status).ToList().Should().AllBeEquivalentTo(DocumentStatus.Indexed);
            tracker.Documents.Select(t => t.CmsDocType.DocumentType == "MG 5").ToList().Should().NotBeEmpty();
            tracker.Documents.Select(t => t.CmsDocType.DocumentType == "PCD").ToList().Should().NotBeEmpty();
            tracker.Documents.Select(t => t.CmsDocType.DocumentType == "DAC").ToList().Should().NotBeEmpty();

            searchExpectations = new (string query, bool exists)[]
              {
                  ("one", true),
                  ("two", true),
                  ("three", false),
                  ("four", true),
                  ("alice", true),
                  ("bob", true),
                  ("carol", false),
                  ("dave", true)
              };

            await CheckSearchAssertions(urn, caseId, e2e0, searchExpectations);
            #endregion
       }

        [Fact]
        public async Task Subsequent_Refresh_Case_Returns_LockedStatus()
        {
            // Arrange
            var urn = "01VK0000421";
            var caseId = 2146928;

            // Act / Assert
            await CaseDelete(urn, caseId);

            var status = await CaseRefresh(urn, caseId, e2e1);
            status.Should().Be(HttpStatusCode.Accepted);

            status = await CaseRefresh(urn, caseId, e2e1);
            status.Should().Be(HttpStatusCode.Locked);
        }

        private async Task CheckSearchAssertions(string urn, int caseId, string e2e0, (string query, bool exists)[] searchAssertions)
        {
            foreach (var (query, exists) in searchAssertions)
            {
                var streamlinedSearchLines = await CaseSearch(urn, caseId, e2e0, query);
                if (exists)
                    streamlinedSearchLines.Should().NotBeEmpty();
                else
                    streamlinedSearchLines.Should().BeEmpty();  
            }
        }
    }
}
#endif