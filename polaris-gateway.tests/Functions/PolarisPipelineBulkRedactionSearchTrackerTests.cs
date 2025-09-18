using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Functions;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PolarisGateway.Tests.Functions;

public class PolarisPipelineBulkRedactionSearchTrackerTests
{
    private readonly Mock<ILogger<PolarisPipelineBulkRedactionSearchTracker>> _loggerMock;
    private readonly Mock<ICoordinatorClient> _coordinatorClientMock;
    private readonly PolarisPipelineBulkRedactionSearchTracker _polarisPipelineBulkRedactionSearchTracker;
    public PolarisPipelineBulkRedactionSearchTrackerTests()
    {
        _loggerMock = new Mock<ILogger<PolarisPipelineBulkRedactionSearchTracker>>();
        _coordinatorClientMock = new Mock<ICoordinatorClient>();
        _polarisPipelineBulkRedactionSearchTracker = new PolarisPipelineBulkRedactionSearchTracker(_loggerMock.Object, _coordinatorClientMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnIActionResult()
    {
        //arrange
        var searchText = "Hello";
        var req = new DefaultHttpContext().Request;
        req.QueryString = new QueryString($"?SearchText={searchText}");
        var caseUrn = "caseUrn";
        var caseId = 1;
        var documentId = "CMS-12345";
        var versionId = 2;
        var cancellationToken = CancellationToken.None;
        _coordinatorClientMock.Setup(s => s.GetTrackerBulkRedactionSearchAsync(caseUrn, caseId, documentId, versionId, searchText, It.IsAny<Guid>(), cancellationToken)).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        //act
        var result = await _polarisPipelineBulkRedactionSearchTracker.Run(req, caseUrn, caseId, documentId, versionId, cancellationToken);

        //assert
        Assert.IsType<StatusCodeResult>(result);
        Assert.Equal((int)HttpStatusCode.OK, (result as StatusCodeResult).StatusCode);
    }
}