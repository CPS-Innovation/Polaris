using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Dto.Response;
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

public class GetWitnessStatementsTests
{
    private readonly Mock<ILogger<GetWitnessStatements>> _loggerMock;
    private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
    private readonly Mock<IDdeiClientFactory> _ddeiClientFactoryMock;
    private readonly GetWitnessStatements _getWitnessStatements;
    public GetWitnessStatementsTests()
    {
        _loggerMock = new Mock<ILogger<GetWitnessStatements>>();
        _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
        _ddeiClientFactoryMock = new Mock<IDdeiClientFactory>();
        _getWitnessStatements = new GetWitnessStatements(_loggerMock.Object, _ddeiArgFactoryMock.Object, _ddeiClientFactoryMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkObjectResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var witnessId = 2;
        var witnessStatementsArgDto = new DdeiWitnessStatementsArgDto();
        var ddeiClientMock = new Mock<IDdeiClient>();
        var witnessStatementDtos = new List<WitnessStatementDto>();
        _ddeiArgFactoryMock.Setup(s => s.CreateWitnessStatementsArgDto(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId, witnessId)).Returns(witnessStatementsArgDto);
        _ddeiClientFactoryMock.Setup(s => s.Create(It.IsAny<string>(), DdeiClients.Mds)).Returns(ddeiClientMock.Object);
        ddeiClientMock.Setup(s => s.GetWitnessStatementsAsync(witnessStatementsArgDto)).ReturnsAsync(witnessStatementDtos);
        //act
        var result = await _getWitnessStatements.Run(req, caseUrn, caseId, witnessId);

        //assert
        Assert.IsType<OkObjectResult>(result);
    }
}