using Common.Dto.Response.Case;
using Ddei.Domain.CaseData.Args;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace PolarisGateway.Tests.Functions;

public class CheckoutDocumentLegacyTests
{
    private readonly Mock<ILogger<CheckoutDocumentLegacy>> _loggerMock;
    private readonly Mock<IMdsArgFactory> _mdsArgFactoryMock;
    private readonly Mock<IMdsClient> _mdsClientMock;
    private readonly CheckoutDocumentLegacy _checkoutDocument;

    public CheckoutDocumentLegacyTests()
    {
        _loggerMock = new Mock<ILogger<CheckoutDocumentLegacy>>();
        _mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        _mdsClientMock = new Mock<IMdsClient>();
        _checkoutDocument = new CheckoutDocumentLegacy(_loggerMock.Object, _mdsArgFactoryMock.Object, _mdsClientMock.Object);
    }

    [Fact]
    public async Task Run_ClientResultIsSuccess_ShouldReturn200()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var materialId = "materialId";
        long documentId = 2;
        var mdsDocumentIdAndVersionIdArgDto = new MdsMaterialIdAndDocumentIdArgDto();
        var checkoutDocumentDto = new CheckoutDocumentDto()
        {
            IsSuccess = true
        };
        _mdsArgFactoryMock.Setup(s => s.CreateDocumentVersionArgDto(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId, materialId, documentId)).Returns(mdsDocumentIdAndVersionIdArgDto);
        _mdsClientMock.Setup(s => s.CheckoutDocumentAsync(mdsDocumentIdAndVersionIdArgDto)).ReturnsAsync(checkoutDocumentDto);

        //act
        var result = await _checkoutDocument.Run(req, caseUrn, caseId, materialId, documentId);

        //assert
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

        _mdsArgFactoryMock
            .Setup(s => s.CreateDocumentVersionArgDto(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                caseUrn,
                caseId,
                materialId,
                documentId))
            .Returns(argDto);

        _mdsClientMock
            .Setup(s => s.CheckoutDocumentAsync(argDto))
            .ThrowsAsync(new HttpRequestException("409 Conflict"));

        // act & assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _checkoutDocument.Run(req, caseUrn, caseId, materialId, documentId));
    }
}