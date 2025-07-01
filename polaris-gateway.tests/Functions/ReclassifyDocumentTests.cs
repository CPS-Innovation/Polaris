using Common.Dto.Request;
using Common.Dto.Response.Document;
using Common.Telemetry;
using Ddei.Domain.CaseData.Args;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using PolarisGateway.Functions;
using PolarisGateway.Mappers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PolarisGateway.Tests.Functions;

public class ReclassifyDocumentTests
{
    private readonly Mock<ILogger<ReclassifyDocument>> _loggerMock;
    private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
    private readonly Mock<IDdeiAuthClient> _ddeiAuthClientMock;
    private readonly Mock<IReclassifyDocumentRequestMapper> _reclassifyDocumentRequestMapperMock;
    private readonly Mock<ITelemetryClient> _telemetryClientMock;
    private readonly ReclassifyDocument _reclassifyDocument;

    public ReclassifyDocumentTests()
    {
        _loggerMock = new Mock<ILogger<ReclassifyDocument>>();
        _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
        _ddeiAuthClientMock = new Mock<IDdeiAuthClient>();
        _reclassifyDocumentRequestMapperMock = new Mock<IReclassifyDocumentRequestMapper>();
        _telemetryClientMock = new Mock<ITelemetryClient>();
        _reclassifyDocument = new ReclassifyDocument(_loggerMock.Object, _ddeiAuthClientMock.Object, _ddeiArgFactoryMock.Object, _reclassifyDocumentRequestMapperMock.Object, _telemetryClientMock.Object);
    }

    [Fact]
    public async Task Run_ReclassifyDocumentIsSuccess_ShouldReturn200()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var documentId = "12345";
        var ddeiReclassifyDocumentArgDto = new DdeiReclassifyDocumentArgDto();
        var reclassifyDocumentDto = new DdeiReclassifyDocumentArgDto()
        {
            DocumentTypeId = 1001,
            Other = new ReclassificationOther
            {
                DocumentName = "New Document Name",
                Used = true,
            }
        };
        req.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(reclassifyDocumentDto)));
        req.ContentType = "application/json";

        var reclassifyResult = new DocumentReclassifiedResultDto
        {
            DocumentId = 12345,
            ReclassificationType = "OTHER",
            OriginalDocumentTypeId = 2001,
            DocumentTypeId = 1001
        };
        _ddeiArgFactoryMock.Setup(s => s.CreateReclassifyDocumentArgDto(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId, documentId, It.IsAny<ReclassifyDocumentDto>())).Returns(ddeiReclassifyDocumentArgDto);
        _ddeiAuthClientMock.Setup(s => s.ReclassifyDocumentAsync(It.IsAny<DdeiReclassifyDocumentArgDto>())).ReturnsAsync(reclassifyResult);

        //act
        var result = await _reclassifyDocument.Run(req, caseUrn, caseId, documentId);

        //assert
        Assert.IsType<ObjectResult>(result);
    }

    [Fact]
    public async Task Run_ReclassifyDocumentInvalidBody_ShouldReturnBadRequestResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var documentId = "12345";
        var ddeiReclassifyDocumentArgDto = new DdeiReclassifyDocumentArgDto();
        var reclassifyDocumentDto = new ReclassifyDocumentDto();
        req.Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(reclassifyDocumentDto)));
        req.ContentType = "application/json";

        //act
        var result = await _reclassifyDocument.Run(req, caseUrn, caseId, documentId);

        //assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(400, statusCodeResult.StatusCode);

    }
}