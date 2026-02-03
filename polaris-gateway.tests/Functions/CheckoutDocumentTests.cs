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
using System.Threading.Tasks;
using Xunit;

namespace PolarisGateway.Tests.Functions;

public class CheckoutDocumentTests
{
    private readonly Mock<ILogger<CheckoutDocument>> _loggerMock;
    private readonly Mock<IMdsArgFactory> _mdsArgFactoryMock;
    private readonly Mock<IMdsClient> _mdsClientMock;
    private readonly CheckoutDocument _checkoutDocument;

    public CheckoutDocumentTests()
    {
        _loggerMock = new Mock<ILogger<CheckoutDocument>>();
        _mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        _mdsClientMock = new Mock<IMdsClient>();
        _checkoutDocument = new CheckoutDocument(_loggerMock.Object, _mdsArgFactoryMock.Object, _mdsClientMock.Object);
    }

    [Fact]
    public async Task Run_ClientResultIsSuccess_ShouldReturn200()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var documentId = "documentId";
        long versionId = 2;
        var ddeiDocumentIdAndVersionIdArgDto = new MdsDocumentIdAndVersionIdArgDto();
        var checkoutDocumentDto = new CheckoutDocumentDto()
        {
            IsSuccess = true
        };
        _mdsArgFactoryMock.Setup(s => s.CreateDocumentVersionArgDto(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId, documentId, versionId)).Returns(ddeiDocumentIdAndVersionIdArgDto);
        _mdsClientMock.Setup(s => s.CheckoutDocumentAsync(ddeiDocumentIdAndVersionIdArgDto)).ReturnsAsync(checkoutDocumentDto);

        //act
        var result = await _checkoutDocument.Run(req, caseUrn, caseId, documentId, versionId);

        //assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Run_ClientResultIsNotSuccess_ShouldReturn409()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var documentId = "documentId";
        long versionId = 2;
        var ddeiDocumentIdAndVersionIdArgDto = new MdsDocumentIdAndVersionIdArgDto();
        var checkoutDocumentDto = new CheckoutDocumentDto()
        {
            IsSuccess = false,
            LockingUserName = "lockingUserName"
        };
        _mdsArgFactoryMock.Setup(s => s.CreateDocumentVersionArgDto(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId, documentId, versionId)).Returns(ddeiDocumentIdAndVersionIdArgDto);
        _mdsClientMock.Setup(s => s.CheckoutDocumentAsync(ddeiDocumentIdAndVersionIdArgDto)).ReturnsAsync(checkoutDocumentDto);

        //act
        var result = await _checkoutDocument.Run(req, caseUrn, caseId, documentId, versionId);

        //assert
        Assert.IsType<ConflictObjectResult>(result);
        var response = result as ConflictObjectResult;
        Assert.Equal(checkoutDocumentDto.LockingUserName, response.Value);
    }
}