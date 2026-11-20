// <copyright file="ToggleIsUnusedDocumentLegacyTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions;

using Common.Dto.Request;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Domain.Args;
using DdeiClient.Enums;
using DdeiClient.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class ToggleIsUnusedDocumentLegacyTests
{
    private readonly Mock<IMasterDataServiceClient> masterDataServiceClientMock;
    private readonly ToggleIsUnusedDocumentLegacy toggleIsUnusedDocument;

    public ToggleIsUnusedDocumentLegacyTests()
    {
        this.masterDataServiceClientMock = new Mock<IMasterDataServiceClient>();
        this.toggleIsUnusedDocument = new ToggleIsUnusedDocumentLegacy(this.masterDataServiceClientMock.Object);
    }

    [Fact]
    public async Task Run_ToggleIsUnUsedDocumentAsyncReturnsTrue_ShouldReturnOkResult()
    {
        // arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        string documentId = "CMS-1";
        var isUnused = "unused";

        this.masterDataServiceClientMock
        .Setup(s => s.ToggleIsUnusedDocumentAsync(
            It.IsAny<MdsToggleIsUnusedDocumentDto>(),
            It.IsAny<CmsAuthValues>(),
            It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

        // act
        var result = await this.toggleIsUnusedDocument.Run(req, caseUrn, caseId, documentId, isUnused);

        // assert
        Assert.IsType<OkResult>(result);
    }
}
