using Common.Dto.Response.Case;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Enums;
using DdeiClient.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PolarisGateway.Tests.Functions;

public class GetCaseTests
{
    private readonly Mock<ILogger<GetCase>> _loggerMock;
    private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
    private readonly Mock<IDdeiClientFactory> _ddeiClientFactoryMock;
    private readonly GetCase _getCase;

    public GetCaseTests()
    {
        _loggerMock = new Mock<ILogger<GetCase>>();
        _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
        _ddeiClientFactoryMock = new Mock<IDdeiClientFactory>();
        _getCase = new GetCase(_loggerMock.Object, _ddeiClientFactoryMock.Object, _ddeiArgFactoryMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkObjectResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var caseIdentifiersArgDto = new DdeiCaseIdentifiersArgDto();
        var ddeiClientMock = new Mock<IDdeiClient>();
        var caseDto = new CaseDto();
        _ddeiArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId)).Returns(caseIdentifiersArgDto);
        _ddeiClientFactoryMock.Setup(s => s.Create(It.IsAny<string>(), DdeiClients.Ddei)).Returns(ddeiClientMock.Object);
        ddeiClientMock.Setup(s => s.GetCaseAsync(caseIdentifiersArgDto)).ReturnsAsync(caseDto);
        //act
        var result = await _getCase.Run(req, caseUrn, caseId);

        //assert
        Assert.IsType<OkObjectResult>(result);
    }
}