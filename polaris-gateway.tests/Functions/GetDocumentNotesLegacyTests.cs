using Common.Dto.Request;
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
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PolarisGateway.Tests.Functions;

public class GetDocumentNotesLegacyTests
{
    private readonly Mock<IMdsArgFactory> _mdsArgFactoryMock;
    private readonly Mock<IMasterDataServiceClient> _masterDataServiceClientMock;
    private readonly GetDocumentNotesLegacy _getGetDocumentNotes;
    public GetDocumentNotesLegacyTests()
    {
        _mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        _masterDataServiceClientMock = new Mock<IMasterDataServiceClient>();
        _getGetDocumentNotes = new GetDocumentNotesLegacy(_masterDataServiceClientMock.Object, _mdsArgFactoryMock.Object);
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
        _masterDataServiceClientMock
            .Setup(s => s.ListDocumentNotesAsync(
                It.IsAny<MdsDocumentArgDto>(),
                It.IsAny<CmsAuthValues>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(documentNoteDtos);
        //act
        var result = await _getGetDocumentNotes.Run(req, caseUrn, caseId, documentId, CancellationToken.None);

        //assert
        Assert.IsType<OkObjectResult>(result);
    }
}
