// <copyright file="PolarisPipelineBulkRedactionSearchTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions;

using System.Net.Http;
using System.Net;
using System.Threading;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Functions;
using Xunit;
using Microsoft.AspNetCore.Mvc;

public class PolarisPipelineBulkRedactionSearchTests
{
    private readonly Mock<ICoordinatorClient> coordinatorClientMock;
    private readonly PolarisPipelineBulkRedactionSearchStart polarisPipelineBulkRedactionSearch;

    public PolarisPipelineBulkRedactionSearchTests()
    {
        this.coordinatorClientMock = new Mock<ICoordinatorClient>();
        this.polarisPipelineBulkRedactionSearch = new PolarisPipelineBulkRedactionSearchStart(this.coordinatorClientMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnResultFromCoordinatorClient()
    {
        // arrange
        var searchText = "Hello";
        var req = new DefaultHttpContext().Request;
        req.QueryString = new QueryString($"?SearchText={searchText}");
        var caseId = 1;
        var materialId = "CMS-12345";
        var documentId = 2;
        var cancellationToken = CancellationToken.None;
        this.coordinatorClientMock.Setup(s => s.BulkRedactionInitiateSearchAsync(caseId, materialId, documentId, It.IsAny<Guid>(), It.IsAny<string>(),cancellationToken)).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // act
        var result = await this.polarisPipelineBulkRedactionSearch.Run(req, caseId, materialId, documentId, cancellationToken);

        // assert
        Assert.IsType<StatusCodeResult>(result);
        Assert.Equal((int)HttpStatusCode.OK, (result as StatusCodeResult).StatusCode);
    }
}
