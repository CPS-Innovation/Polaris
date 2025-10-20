using Common.Configuration;
using coordinator.Domain;
using coordinator.Durable.Payloads;
using coordinator.Durable.Providers;
using coordinator.Functions;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace coordinator.tests.Functions;

public class BulkRedactionSearchTests
{
    private readonly Mock<ILogger<BulkRedactionSearch>> _loggerMock;
    private readonly Mock<IOrchestrationProvider> _orchestrationProviderMock;
    private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
    private readonly Mock<IDdeiAuthClient> _ddeiAuthClientMock;
    private readonly BulkRedactionSearch _bulkRedactionSearch;

    public BulkRedactionSearchTests()
    {
        _loggerMock = new Mock<ILogger<BulkRedactionSearch>>();
        _orchestrationProviderMock = new Mock<IOrchestrationProvider>();
        _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
        _ddeiAuthClientMock = new Mock<IDdeiAuthClient>();
        _bulkRedactionSearch = new BulkRedactionSearch(_loggerMock.Object, _orchestrationProviderMock.Object, _ddeiArgFactoryMock.Object, _ddeiAuthClientMock.Object);
    }

    [Fact]
    public async Task Run_IsAccepted_ShouldOkResponse()
    {
        //arrange
        var searchText = "Hello";
        var req = new DefaultHttpContext().Request;
        req.Headers.Add("Correlation-Id", Guid.NewGuid().ToString());
        req.Headers.Add("Cms-Auth-Values", "Cms-Auth-Values");
        req.QueryString = new QueryString($"?SearchText={searchText}");
        var caseUrn = "caseUrn";
        var caseId = 1;
        var documentId = "CMS-12345";
        var versionId = 2;
        var cancellationToken = CancellationToken.None;
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var ddeiBaseArgDto = new DdeiBaseArgDto();
        _ddeiArgFactoryMock.Setup(s => s.CreateCmsCaseDataArgDto(It.IsAny<string>(), It.IsAny<Guid>())).Returns(ddeiBaseArgDto);
        _orchestrationProviderMock.Setup(s => s.BulkSearchDocumentAsync(orchestrationClientMock.Object, It.IsAny<BulkRedactionSearchPayload>(), cancellationToken)).ReturnsAsync(true);
        //act
        var result = await _bulkRedactionSearch.Run(req, caseUrn, caseId, documentId, versionId, cancellationToken, orchestrationClientMock.Object);

        //assert
        _ddeiAuthClientMock.Verify(v => v.VerifyCmsAuthAsync(ddeiBaseArgDto), Times.Once);
        Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.OK, (result as ObjectResult).StatusCode);
        Assert.Equal($"/api/{RestApi.GetBulkRedactionSearchTrackerPath(caseUrn, caseId, documentId, versionId, searchText)}", ((BulkRedactionSearchResponse)(result as ObjectResult).Value).TrackerUrl);
    }

    [Fact]
    public async Task Run_IsNotAccepted_ShouldLockedResponse()
    {
        //arrange
        var searchText = "Hello";
        var req = new DefaultHttpContext().Request;
        req.Headers.Add("Correlation-Id", Guid.NewGuid().ToString());
        req.Headers.Add("Cms-Auth-Values", "Cms-Auth-Values");
        req.QueryString = new QueryString($"?SearchText={searchText}");
        var caseUrn = "caseUrn";
        var caseId = 1;
        var documentId = "CMS-12345";
        var versionId = 2;
        var cancellationToken = CancellationToken.None;
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var ddeiBaseArgDto = new DdeiBaseArgDto();
        _ddeiArgFactoryMock.Setup(s => s.CreateCmsCaseDataArgDto(It.IsAny<string>(), It.IsAny<Guid>())).Returns(ddeiBaseArgDto);
        _orchestrationProviderMock.Setup(s => s.BulkSearchDocumentAsync(orchestrationClientMock.Object, It.IsAny<BulkRedactionSearchPayload>(), cancellationToken)).ReturnsAsync(false);
        //act
        var result = await _bulkRedactionSearch.Run(req, caseUrn, caseId, documentId, versionId, cancellationToken, orchestrationClientMock.Object);

        //assert
        _ddeiAuthClientMock.Verify(v => v.VerifyCmsAuthAsync(ddeiBaseArgDto), Times.Once);
        Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)HttpStatusCode.Locked, (result as ObjectResult).StatusCode);
        Assert.Equal($"/api/{RestApi.GetBulkRedactionSearchTrackerPath(caseUrn, caseId, documentId, versionId, searchText)}", ((BulkRedactionSearchResponse)(result as ObjectResult).Value).TrackerUrl);
    }
}