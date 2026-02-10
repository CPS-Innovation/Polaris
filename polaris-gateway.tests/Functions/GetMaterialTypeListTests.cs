using Common.Dto.Response;
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

public class GetMaterialTypeListTests
{
    private readonly Mock<ILogger<GetMaterialTypeList>> _loggerMock;
    private readonly Mock<IMdsArgFactory> _mdsArgFactoryMock;
    private readonly Mock<IMdsClient> _mdsClientMock;
    private readonly GetMaterialTypeList _getMaterialTypeList;

    public GetMaterialTypeListTests()
    {
        _loggerMock = new Mock<ILogger<GetMaterialTypeList>>();
        _mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        _mdsClientMock = new Mock<IMdsClient>();
        _getMaterialTypeList = new GetMaterialTypeList(_loggerMock.Object, _mdsArgFactoryMock.Object, _mdsClientMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkObjectResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var cmsBaseArgDto = new CmsBaseArgDto();
        var materialTypes = new List<MaterialTypeDto>();
        _mdsArgFactoryMock.Setup(s => s.CreateCmsCaseDataArgDto(It.IsAny<string>(), It.IsAny<Guid>())).Returns(cmsBaseArgDto);
        _mdsClientMock.Setup(s => s.GetMaterialTypeListAsync(cmsBaseArgDto)).ReturnsAsync(materialTypes);
        
        //act
        var result = await _getMaterialTypeList.Run(req);

        //assert
        Assert.IsType<OkObjectResult>(result);
    }
}