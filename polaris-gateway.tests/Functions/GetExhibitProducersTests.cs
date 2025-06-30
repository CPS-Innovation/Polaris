using Common.Dto.Response.Case;
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

public class GetExhibitProducersTests
{
    private readonly Mock<ILogger<GetExhibitProducers>> _loggerMock;
    private readonly Mock<IMdsClient> _mdsClientMock;
    private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
    private readonly GetExhibitProducers _getExhibitProducers;

    public GetExhibitProducersTests()
    {
        _loggerMock = new Mock<ILogger<GetExhibitProducers>>();
        _mdsClientMock = new Mock<IMdsClient>();
        _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
        _getExhibitProducers = new GetExhibitProducers(_loggerMock.Object, _mdsClientMock.Object, _ddeiArgFactoryMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkObjectResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var ddeiCaseIdentifiersArgDto = new DdeiCaseIdentifiersArgDto();
        var exhibitProducerDtos = new List<ExhibitProducerDto>();
        _ddeiArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId)).Returns(ddeiCaseIdentifiersArgDto);
        _mdsClientMock.Setup(s => s.GetExhibitProducersAsync(ddeiCaseIdentifiersArgDto)).ReturnsAsync(exhibitProducerDtos);

        //act
        var result = await _getExhibitProducers.Run(req, caseUrn, caseId);

        //assert
        Assert.IsType<OkObjectResult>(result);
    }
}