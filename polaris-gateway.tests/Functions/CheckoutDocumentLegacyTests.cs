// <copyright file="CheckoutDocumentLegacyTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions;

using Common.Dto.Request;
using Common.Dto.Response.Case;
using Ddei.Domain.CaseData.Args;
using Ddei.Factories;
using DdeiClient.Clients;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class CheckoutDocumentLegacyTests
{
    private readonly Mock<IMdsArgFactory> mdsArgFactoryMock;
    private readonly Mock<IMasterDataServiceClient> masterDataServiceClientMock;
    private readonly CheckoutDocumentLegacy checkoutDocument;

    public CheckoutDocumentLegacyTests()
    {
        this.mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        this.masterDataServiceClientMock = new Mock<IMasterDataServiceClient>();
        this.checkoutDocument = new CheckoutDocumentLegacy(this.mdsArgFactoryMock.Object, this.masterDataServiceClientMock.Object);
    }

    [Fact]
    public async Task Run_ClientResultIsSuccess_ShouldReturn200()
    {
        // arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var materialId = "materialId";
        long documentId = 2;
        var mdsDocumentIdAndVersionIdArgDto = new MdsMaterialIdAndDocumentIdArgDto();
        var checkoutDocumentDto = new CheckoutDocumentDto()
        {
            IsSuccess = true,
        };
        this.mdsArgFactoryMock.Setup(s => s.CreateDocumentVersionArgDto(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId, materialId, documentId)).Returns(mdsDocumentIdAndVersionIdArgDto);
        this.masterDataServiceClientMock
            .Setup(s => s.CheckoutDocumentAsync(
                mdsDocumentIdAndVersionIdArgDto,
                It.IsAny<CmsAuthValues>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkoutDocumentDto);

        // act
        var result = await this.checkoutDocument.Run(req, caseUrn, caseId, materialId, documentId);

        // assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Run_WhenClientThrowsConflict_ShouldBubbleException()
    {
        // arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var materialId = "materialId";
        long documentId = 2;

        var argDto = new MdsMaterialIdAndDocumentIdArgDto();

        this.mdsArgFactoryMock
            .Setup(s => s.CreateDocumentVersionArgDto(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                caseUrn,
                caseId,
                materialId,
                documentId))
            .Returns(argDto);

        this.masterDataServiceClientMock
            .Setup(s => s.CheckoutDocumentAsync(
                argDto,
                It.IsAny<CmsAuthValues>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("409 Conflict"));

        // act & assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            this.checkoutDocument.Run(req, caseUrn, caseId, materialId, documentId));
    }
}
