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
    private readonly Mock<IMdsArgFactory> _mdsArgFactoryMock;
    private readonly GetExhibitProducers _getExhibitProducers;

    public GetExhibitProducersTests()
    {
        _loggerMock = new Mock<ILogger<GetExhibitProducers>>();
        _mdsClientMock = new Mock<IMdsClient>();
        _mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        _getExhibitProducers = new GetExhibitProducers(_loggerMock.Object, _mdsClientMock.Object, _mdsArgFactoryMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkObjectResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();
        var exhibitProducerDtos = new List<ExhibitProducerDto>();
        _mdsArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId)).Returns(mdsCaseIdentifiersArgDto);
        _mdsClientMock.Setup(s => s.GetExhibitProducersAsync(mdsCaseIdentifiersArgDto)).ReturnsAsync(exhibitProducerDtos);

        //act
        var result = await _getExhibitProducers.Run(req, caseUrn, caseId);

        //assert
        Assert.IsType<OkObjectResult>(result);
    }
}