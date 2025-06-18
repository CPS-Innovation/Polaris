﻿using Common.Dto.Response.Case;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
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
    private readonly Mock<IDdeiCaseOrchestrationService> _ddeiCaseOrchestrationServiceMock;
    private readonly GetCase _getCase;

    public GetCaseTests()
    {
        _loggerMock = new Mock<ILogger<GetCase>>();
        _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
        _ddeiCaseOrchestrationServiceMock = new Mock<IDdeiCaseOrchestrationService>();
        _getCase = new GetCase(_loggerMock.Object, _ddeiArgFactoryMock.Object, _ddeiCaseOrchestrationServiceMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkObjectResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var caseIdentifiersArgDto = new DdeiCaseIdentifiersArgDto();
        var caseDetails = new CaseDetailsDto();
        var caseDto = new CaseDto();
        _ddeiArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId)).Returns(caseIdentifiersArgDto);
        _ddeiCaseOrchestrationServiceMock.Setup(s => s.GetCase(caseIdentifiersArgDto)).ReturnsAsync(caseDetails);

        //act
        var result = await _getCase.Run(req, caseUrn, caseId);

        //assert
        Assert.IsType<OkObjectResult>(result);
    }
}