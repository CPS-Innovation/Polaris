using Common.Dto.Response.Case;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions;
using PolarisGateway.Services.MdsOrchestration;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PolarisGateway.Tests.Functions;

public class GetCaseLegacyTests
{
    private readonly Mock<ILogger<GetCaseLegacy>> _loggerMock;
    private readonly Mock<IMdsArgFactory> _mdsArgFactoryMock;
    private readonly Mock<IMdsCaseOrchestrationService> _mdsCaseOrchestrationServiceMock;
    private readonly GetCaseLegacy _getCase;

    public GetCaseLegacyTests()
    {
        _loggerMock = new Mock<ILogger<GetCaseLegacy>>();
        _mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        _mdsCaseOrchestrationServiceMock = new Mock<IMdsCaseOrchestrationService>();
        _getCase = new GetCaseLegacy(_loggerMock.Object, _mdsArgFactoryMock.Object, _mdsCaseOrchestrationServiceMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkObjectResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var caseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();
        var caseDetails = new CaseDetailsDto();
        var caseDto = new CaseDto();
        _mdsArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId)).Returns(caseIdentifiersArgDto);
        _mdsCaseOrchestrationServiceMock.Setup(s => s.GetCase(caseIdentifiersArgDto)).ReturnsAsync(caseDto);

        //act
        var result = await _getCase.Run(req, caseUrn, caseId);

        //assert
        Assert.IsType<OkObjectResult>(result);
    }
}