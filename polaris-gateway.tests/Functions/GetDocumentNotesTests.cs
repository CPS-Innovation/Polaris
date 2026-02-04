using Common.Dto.Response.Document;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PolarisGateway.Tests.Functions;

public class GetDocumentNotesTests
{
    private readonly Mock<ILogger<GetDocumentNotes>> _loggerMock;
    private readonly Mock<IMdsArgFactory> _mdsArgFactoryMock;
    private readonly Mock<IMdsClient> _mdsClientMock;
    private readonly GetDocumentNotes _getGetDocumentNotes;
    public GetDocumentNotesTests()
    {
        _loggerMock = new Mock<ILogger<GetDocumentNotes>>();
        _mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        _mdsClientMock = new Mock<IMdsClient>();
        _getGetDocumentNotes = new GetDocumentNotes(_loggerMock.Object, _mdsClientMock.Object, _mdsArgFactoryMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkObjectResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var documentId = "CMS-1234";
        var documentArgDto = new MdsDocumentArgDto();
        
        var documentNoteDtos = new List<DocumentNoteDto>();
        _mdsArgFactoryMock.Setup(s => s.CreateDocumentArgDto(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId, documentId)).Returns(documentArgDto);
        _mdsClientMock.Setup(s => s.GetDocumentNotesAsync(documentArgDto)).ReturnsAsync(documentNoteDtos);
        //act
        var result = await _getGetDocumentNotes.Run(req, caseUrn, caseId, documentId);

        //assert
        Assert.IsType<OkObjectResult>(result);
    }
}