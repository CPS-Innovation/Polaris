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
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PolarisGateway.Tests.Functions;

public class GetCasesTests
{
    private readonly Mock<ILogger<GetCases>> _loggerMock;
    private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
    private readonly Mock<IDdeiClientFactory> _ddeiClientFactoryMock;
    private readonly GetCases _getGetCases;

    public GetCasesTests()
    {
        _loggerMock = new Mock<ILogger<GetCases>>();
        _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
        _ddeiClientFactoryMock = new Mock<IDdeiClientFactory>();
        _getGetCases = new GetCases(_loggerMock.Object, _ddeiClientFactoryMock.Object, _ddeiArgFactoryMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkObjectResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var urnArgDto = new DdeiUrnArgDto();
        var ddeiClientMock = new Mock<IDdeiClient>();
        var caseDtos = new List<CaseDto>();
        _ddeiArgFactoryMock.Setup(s => s.CreateUrnArg(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn)).Returns(urnArgDto);
        _ddeiClientFactoryMock.Setup(s => s.Create(It.IsAny<string>(), DdeiClients.Mds)).Returns(ddeiClientMock.Object);
        ddeiClientMock.Setup(s => s.ListCasesAsync(urnArgDto)).ReturnsAsync(caseDtos);
        //act
        var result = await _getGetCases.Run(req, caseUrn);

        //assert
        Assert.IsType<OkObjectResult>(result);
    }
}