// <copyright file="CancelCheckoutDocumentLegacyTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions;

using Common.Dto.Request;
using Ddei.Domain.CaseData.Args;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class CancelCheckoutDocumentLegacyTests
{
    private readonly Mock<IMdsArgFactory> mdsArgFactoryMock;
    private readonly Mock<IMasterDataServiceClient> masterDataServiceClientMock;
    private readonly CancelCheckoutDocumentLegacy cancelCheckoutDocument;

    public CancelCheckoutDocumentLegacyTests()
    {
        this.mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        this.masterDataServiceClientMock = new Mock<IMasterDataServiceClient>();
        this.cancelCheckoutDocument = new CancelCheckoutDocumentLegacy(this.mdsArgFactoryMock.Object, this.masterDataServiceClientMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkResult()
    {
        // arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var materialId = "materialId";
        long documentId = 2;
        var mdsDocumentIdAndVersionIdArgDto = new MdsMaterialIdAndDocumentIdArgDto();
        this.mdsArgFactoryMock.Setup(s => s.CreateDocumentVersionArgDto(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId, materialId, documentId)).Returns(mdsDocumentIdAndVersionIdArgDto);

        // act
        var result = await this.cancelCheckoutDocument.Run(req, caseUrn, caseId, materialId, documentId);

        // assert
        this.masterDataServiceClientMock.Verify(
            v => v.CancelCheckoutDocumentAsync(
                mdsDocumentIdAndVersionIdArgDto,
                It.IsAny<CmsAuthValues>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.IsType<OkResult>(result);
    }
}
