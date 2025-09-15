using System.Net.Http;
using System.Net;
using System.Threading;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Functions;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace PolarisGateway.Tests.Functions;

public class PolarisPipelineBulkRedactionSearchTests
{
    private readonly Mock<ILogger<PolarisPipelineBulkRedactionSearch>> _loggerMock;
    private readonly Mock<ICoordinatorClient> _coordinatorClientMock;
    private readonly PolarisPipelineBulkRedactionSearch _polarisPipelineBulkRedactionSearch;
    public PolarisPipelineBulkRedactionSearchTests()
    {
        _loggerMock = new Mock<ILogger<PolarisPipelineBulkRedactionSearch>>();
        _coordinatorClientMock = new Mock<ICoordinatorClient>();
        _polarisPipelineBulkRedactionSearch = new PolarisPipelineBulkRedactionSearch(_loggerMock.Object, _coordinatorClientMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnResultFromCoordinatorClient()
    {
        //arrange
        //arrange
        var searchText = "Hello";
        var req = new DefaultHttpContext().Request;
        req.QueryString = new QueryString($"?SearchText={searchText}");
        var caseUrn = "caseUrn";
        var caseId = 1;
        var documentId = "CMS-12345";
        var versionId = 2;
        var cancellationToken = CancellationToken.None;
        _coordinatorClientMock.Setup(s => s.BulkRedactionSearchAsync(caseUrn, caseId, documentId, versionId, searchText, It.IsAny<Guid>(), It.IsAny<string>(),cancellationToken)).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        //act
        var result = await _polarisPipelineBulkRedactionSearch.Run(req, caseUrn, caseId, documentId, versionId, cancellationToken);

        //assert
        Assert.IsType<StatusCodeResult>(result);
        Assert.Equal((int)HttpStatusCode.OK, (result as StatusCodeResult).StatusCode);
    }
}