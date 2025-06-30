using System.Threading.Tasks;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Domain.Args;
using DdeiClient.Enums;
using DdeiClient.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions;
using Xunit;

namespace PolarisGateway.Tests.Functions;

public class ToggleIsUnusedDocumentTests
{
    private readonly Mock<ILogger<ToggleIsUnusedDocument>> _loggerMock;
    private readonly Mock<IMdsClient> _mdsClientMock;
    private readonly ToggleIsUnusedDocument _toggleIsUnusedDocument;

    public ToggleIsUnusedDocumentTests()
    {
        _loggerMock = new Mock<ILogger<ToggleIsUnusedDocument>>();
        _mdsClientMock = new Mock<IMdsClient>();
        _toggleIsUnusedDocument = new ToggleIsUnusedDocument(_loggerMock.Object, _mdsClientMock.Object);
    }

    [Fact]
    public async Task Run_ToggleIsUnUsedDocumentAsyncReturnsTrue_ShouldReturnOkResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        string documentId = "CMS-1";
        var isUnused = "unused";
        _mdsClientMock.Setup(s => s.ToggleIsUnusedDocumentAsync(It.IsAny<DdeiToggleIsUnusedDocumentDto>())).ReturnsAsync(true);

        //act
        var result = await _toggleIsUnusedDocument.Run(req, caseUrn, caseId, documentId, isUnused);

        //assert
        Assert.IsType<OkResult>(result);
    }
    
    [Fact]
    public async Task Run_ToggleIsUnUsedDocumentAsyncReturnsFalse_ShouldReturnBadRequestResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        string documentId = "CMS-1";
        var isUnused = "unused";
        _mdsClientMock.Setup(s => s.ToggleIsUnusedDocumentAsync(It.IsAny<DdeiToggleIsUnusedDocumentDto>())).ReturnsAsync(false);

        //act
        var result = await _toggleIsUnusedDocument.Run(req, caseUrn, caseId, documentId, isUnused);

        //assert
        Assert.IsType<BadRequestResult>(result);
    }
}