using Common.Dto.Response.Case;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions;
using PolarisGateway.Services.DdeiOrchestration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PolarisGateway.Tests.Functions;

public class GetCasesTests
{
    private readonly Mock<ILogger<GetCases>> _loggerMock;
    private readonly Mock<IMdsArgFactory> _mdsArgFactoryMock;
    private readonly Mock<IDdeiCaseOrchestrationService> _ddeiCaseOrchestrationServiceMock;
    private readonly GetCases _getCases;

    public GetCasesTests()
    {
        _loggerMock = new Mock<ILogger<GetCases>>();
        _mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        _ddeiCaseOrchestrationServiceMock = new Mock<IDdeiCaseOrchestrationService>();
        _getCases = new GetCases(_loggerMock.Object, _mdsArgFactoryMock.Object, _ddeiCaseOrchestrationServiceMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkObjectResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var urnArgDto = new MdsUrnArgDto();
        var caseDtos = new List<CaseDto>();
        _mdsArgFactoryMock.Setup(s => s.CreateUrnArg(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn)).Returns(urnArgDto);
        _ddeiCaseOrchestrationServiceMock.Setup(s => s.GetCases(urnArgDto)).ReturnsAsync(caseDtos);

        //act
        var result = await _getCases.Run(req, caseUrn);

        //assert
        Assert.IsType<OkObjectResult>(result);
    }
}