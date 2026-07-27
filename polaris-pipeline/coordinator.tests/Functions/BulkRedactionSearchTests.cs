// <copyright file="BulkRedactionSearchTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.tests.Functions;

using coordinator.Domain;
using coordinator.Enums;
using coordinator.Functions;
using coordinator.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DurableTask.Client;
using Moq;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Common.Dto.Request;
using Xunit;

public class BulkRedactionSearchTests
{
    private readonly Mock<IBulkRedactionSearchService> bulkRedactionSearchServiceMock;
    private readonly BulkRedactionSearchStart bulkRedactionSearch;

    public BulkRedactionSearchTests()
    {
        this.bulkRedactionSearchServiceMock = new Mock<IBulkRedactionSearchService>();
        this.bulkRedactionSearch = new BulkRedactionSearchStart(this.bulkRedactionSearchServiceMock.Object);
    }

    [Theory]
    [InlineData(OrchestrationProviderStatus.Initiated, HttpStatusCode.Accepted)]
    [InlineData(OrchestrationProviderStatus.Processing, HttpStatusCode.Accepted)]
    [InlineData(OrchestrationProviderStatus.Completed, HttpStatusCode.OK)]
    [InlineData(OrchestrationProviderStatus.Failed, HttpStatusCode.NotFound)]
    [InlineData(OrchestrationProviderStatus.NotStarted, HttpStatusCode.BadRequest)]
    public async Task Run_BulkRedactionSearchReturnsInitiated_ShouldReturnAccepted(OrchestrationProviderStatus status, HttpStatusCode expectedStatusCode)
    {
        // arrange
        var searchText = "Hello";
        var req = new DefaultHttpContext().Request;
        var correlationId = Guid.NewGuid();
        var cmsAuthValues = "Cms-auth-values";
        req.Headers["Correlation-Id"] = correlationId.ToString();
        req.Headers["Cms-Auth-Values"] = cmsAuthValues;
        req.QueryString = new QueryString($"?SearchText={searchText}");
        var caseUrn = "caseUrn";
        var caseId = 1;
        var materialId = "CMS-12345";
        var documentId = 2;
        var cancellationToken = CancellationToken.None;
        var orchestrationClientMock = new Mock<DurableTaskClient>("name");
        var bulkRedactionSearchResponse = new BulkRedactionSearchResponse()
        {
            DocumentRefreshStatus = status,
        };

        this.bulkRedactionSearchServiceMock.Setup(s => s.InitiateOrOrchestrateOcr(It.IsAny<BulkRedactionSearchDto>(), orchestrationClientMock.Object, cancellationToken)).ReturnsAsync(bulkRedactionSearchResponse);

        // act
        var result = await this.bulkRedactionSearch.Run(req, caseUrn, caseId, materialId, documentId, cancellationToken, orchestrationClientMock.Object);

        // assert
        Assert.IsType<ObjectResult>(result);
        Assert.Equal((int)expectedStatusCode, (result as ObjectResult).StatusCode);
        Assert.Same(bulkRedactionSearchResponse, (BulkRedactionSearchResponse)(result as ObjectResult).Value);
    }
}
