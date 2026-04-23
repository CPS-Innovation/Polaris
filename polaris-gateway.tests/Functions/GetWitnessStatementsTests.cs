using Common.Dto.Response;
using Common.Dto.Response;
using Ddei.Domain.CaseData.Args;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PolarisGateway.Tests.Functions;

public class GetWitnessStatementsTests
{
    private readonly Mock<ILogger<GetWitnessStatements>> _loggerMock;
    private readonly Mock<IMdsArgFactory> _mdsArgFactoryMock;
    private readonly Mock<IMdsClient> _mdsClientMock;
    private readonly GetWitnessStatements _getWitnessStatements;
    public GetWitnessStatementsTests()
    {
        _loggerMock = new Mock<ILogger<GetWitnessStatements>>();
        _mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        _mdsClientMock = new Mock<IMdsClient>();
        _getWitnessStatements = new GetWitnessStatements(_loggerMock.Object, _mdsArgFactoryMock.Object, _mdsClientMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkObjectResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var witnessId = 2;
        var witnessStatementsArgDto = new MdsWitnessStatementsArgDto();
        var witnessStatementDtos = new List<WitnessStatementDto>();
        _mdsArgFactoryMock.Setup(s => s.CreateWitnessStatementsArgDto(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId, witnessId)).Returns(witnessStatementsArgDto);
        _mdsClientMock.Setup(s => s.GetWitnessStatementsAsync(witnessStatementsArgDto)).ReturnsAsync(witnessStatementDtos);
        //act
        var result = await _getWitnessStatements.Run(req, caseUrn, caseId, witnessId);

        //assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Run_ShouldReturnWitnessStatementsWithParentIdMatchingDocumentId()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var witnessId = 2;
        var witnessStatementsArgDto = new MdsWitnessStatementsArgDto();
        var witnessStatementDtos = new List<WitnessStatementDto>
        {
            new WitnessStatementDto { DocumentId = 100, StatementNumber = 1 },
            new WitnessStatementDto { DocumentId = 200, StatementNumber = 2 },
            new WitnessStatementDto { DocumentId = null, StatementNumber = 3 }
        };
        _mdsArgFactoryMock.Setup(s => s.CreateWitnessStatementsArgDto(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId, witnessId)).Returns(witnessStatementsArgDto);
        _mdsClientMock.Setup(s => s.GetWitnessStatementsAsync(witnessStatementsArgDto)).ReturnsAsync(witnessStatementDtos);

        //act
        var result = await _getWitnessStatements.Run(req, caseUrn, caseId, witnessId);

        //assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var statements = Assert.IsAssignableFrom<List<WitnessStatementDto>>(okResult.Value);
        Assert.Equal(3, statements.Count);
        Assert.Equal(100, statements[0].ParentId);
        Assert.Equal(100, statements[0].DocumentId);
        Assert.Equal(200, statements[1].ParentId);
        Assert.Equal(200, statements[1].DocumentId);
        Assert.Null(statements[2].ParentId);
        Assert.Null(statements[2].DocumentId);
    }
}