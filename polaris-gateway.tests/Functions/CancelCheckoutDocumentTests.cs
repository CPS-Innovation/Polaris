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

public class CancelCheckoutDocumentTests
{
    private readonly Mock<ILogger<CancelCheckoutDocument>> _loggerMock;
    private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
    private readonly Mock<IMdsClient> _mdsClientMock;
    private readonly CancelCheckoutDocument _cancelCheckoutDocument;

    public CancelCheckoutDocumentTests()
    {
        _loggerMock = new Mock<ILogger<CancelCheckoutDocument>>();
        _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
        _mdsClientMock = new Mock<IMdsClient>();
        _cancelCheckoutDocument = new CancelCheckoutDocument(_loggerMock.Object, _ddeiArgFactoryMock.Object, _mdsClientMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var documentId = "documentId";
        long versionId = 2;
        var ddeiDocumentIdAndVersionIdArgDto = new DdeiDocumentIdAndVersionIdArgDto();
        _ddeiArgFactoryMock.Setup(s => s.CreateDocumentVersionArgDto(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId, documentId, versionId)).Returns(ddeiDocumentIdAndVersionIdArgDto);
        
        
        //act
        var result = await _cancelCheckoutDocument.Run(req, caseUrn, caseId, documentId, versionId);

        //assert
        _mdsClientMock.Verify(v => v.CancelCheckoutDocumentAsync(ddeiDocumentIdAndVersionIdArgDto), Times.Once);
        Assert.IsType<OkResult>(result);
    }
}