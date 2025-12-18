using coordinator.Domain;
using coordinator.Enums;
using coordinator.Functions;
using coordinator.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Common.Dto.Request;
using Xunit;

namespace coordinator.tests.Functions;

public class BulkRedactionSearchTests
{
    private readonly Mock<ILogger<BulkRedactionSearch>> _loggerMock;
    private readonly Mock<IBulkRedactionSearchService> _bulkRedactionSearchServiceMock;
    private readonly BulkRedactionSearch _bulkRedactionSearch;

    public BulkRedactionSearchTests()
    {
        _loggerMock = new Mock<ILogger<BulkRedactionSearch>>();
        _bulkRedactionSearchServiceMock = new Mock<IBulkRedactionSearchService>();
        _bulkRedactionSearch = new BulkRedactionSearch(_loggerMock.Object, _bulkRedactionSearchServiceMock.Object);
    }

    [Theory]
    [InlineData(OrchestrationProviderStatus.Initiated, HttpStatusCode.Accepted)]
    [InlineData(OrchestrationProviderStatus.Processing, HttpStatusCode.Locked)]
    [InlineData(OrchestrationProviderStatus.Completed, HttpStatusCode.OK)]
    [InlineData(OrchestrationProviderStatus.Failed, HttpStatusCode.InternalServerError)]
    public async Task Run_BulkRedactionSearchReturnsInitiated_ShouldReturnAccepted(OrchestrationProviderStatus status, HttpStatusCode expectedStatusCode)
    {
        //arrange
        var searchText = "Hello";
        var req = new DefaultHttpContext().Request;
        var correlationId = Guid.NewGuid();
        var cmsAuthValues = "Cms-auth-values";
        req.Headers.Add("Correlation-Id", correlationId.ToString());
        req.Headers.Add("Cms-Auth-Values", cmsAuthValues);
        req.QueryString = new QueryString($"?SearchText={searchText}");
        var caseUrn = "caseUrn";
        var caseId = 1;
        var documentId = "CMS-12345";
        var versionId = 2;
        var cancellationToken = CancellationToken.None;
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse()
        {
            DocumentRefreshStatus = status
        };

        _bulkRedactionSearchServiceMock.Setup(s => s.BulkRedactionSearchAsync(It.IsAny<BulkRedactionSearchDto>(), orchestrationClientMock.Object, cancellationToken)).ReturnsAsync(bulkRedactionSearchResponse);

        //act
        var result = await _bulkRedactionSearch.Run(req, caseUrn, caseId, documentId, versionId, cancellationToken, orchestrationClientMock.Object);

        //assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)expectedStatusCode, (result as ObjectResult).StatusCode);
        Assert.Same(bulkRedactionSearchResponse, (BulkRedactionSearchResponse)(result as ObjectResult).Value);
    }
}