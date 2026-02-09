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
}