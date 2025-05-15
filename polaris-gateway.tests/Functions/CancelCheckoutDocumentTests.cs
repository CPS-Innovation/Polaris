using System;
using System.Threading.Tasks;
using Ddei.Domain.CaseData.Args;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Enums;
using DdeiClient.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions;
using Xunit;

namespace PolarisGateway.Tests.Functions;

public class CancelCheckoutDocumentTests
{
    private readonly Mock<ILogger<CancelCheckoutDocument>> _loggerMock;
    private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
    private readonly Mock<IDdeiClientFactory> _ddeiClientFactoryMock;
    private readonly CancelCheckoutDocument _cancelCheckoutDocument;

    public CancelCheckoutDocumentTests()
    {
        _loggerMock = new Mock<ILogger<CancelCheckoutDocument>>();
        _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
        _ddeiClientFactoryMock = new Mock<IDdeiClientFactory>();
        _cancelCheckoutDocument = new CancelCheckoutDocument(_loggerMock.Object, _ddeiArgFactoryMock.Object, _ddeiClientFactoryMock.Object);
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
        var ddeiClientMock = new Mock<IDdeiClient>();
        _ddeiArgFactoryMock.Setup(s => s.CreateDocumentVersionArgDto(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId, documentId, versionId)).Returns(ddeiDocumentIdAndVersionIdArgDto);
        _ddeiClientFactoryMock.Setup(s => s.Create(It.IsAny<string>(), DdeiClients.Mds)).Returns(ddeiClientMock.Object);
        
        //act
        var result = await _cancelCheckoutDocument.Run(req, caseUrn, caseId, documentId, versionId);

        //assert
        ddeiClientMock.Verify(v => v.CancelCheckoutDocumentAsync(ddeiDocumentIdAndVersionIdArgDto), Times.Once);
        Assert.IsType<OkResult>(result);
    }
}