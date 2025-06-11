using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Dto.Response;
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
using Xunit;

namespace PolarisGateway.Tests.Functions;

public class GetMaterialTypeListTests
{
    private readonly Mock<ILogger<GetMaterialTypeList>> _loggerMock;
    private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
    private readonly Mock<IDdeiClientFactory> _ddeiClientFactoryMock;
    private readonly GetMaterialTypeList _getMaterialTypeList;

    public GetMaterialTypeListTests()
    {
        _loggerMock = new Mock<ILogger<GetMaterialTypeList>>();
        _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
        _ddeiClientFactoryMock = new Mock<IDdeiClientFactory>();
        _getMaterialTypeList = new GetMaterialTypeList(_loggerMock.Object, _ddeiArgFactoryMock.Object, _ddeiClientFactoryMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkObjectResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var ddeiBaseArgDto = new DdeiBaseArgDto();
        var ddeiClientMock = new Mock<IDdeiClient>();
        var materialTypes = new List<MaterialTypeDto>();
        _ddeiArgFactoryMock.Setup(s => s.CreateCmsCaseDataArgDto(It.IsAny<string>(), It.IsAny<Guid>())).Returns(ddeiBaseArgDto);
        _ddeiClientFactoryMock.Setup(s => s.Create(It.IsAny<string>(), DdeiClients.Mds)).Returns(ddeiClientMock.Object);
        ddeiClientMock.Setup(s => s.GetMaterialTypeListAsync(ddeiBaseArgDto)).ReturnsAsync(materialTypes);
        
        //act
        var result = await _getMaterialTypeList.Run(req);

        //assert
        Assert.IsType<OkObjectResult>(result);
    }
}