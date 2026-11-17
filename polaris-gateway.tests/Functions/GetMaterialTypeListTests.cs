// <copyright file="GetMaterialTypeListTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions;

using Common.Dto.Request;
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
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class GetMaterialTypeListTests
{
    private readonly Mock<IMdsArgFactory> mdsArgFactoryMock;
    private readonly Mock<IMasterDataServiceClient> masterDataServiceClientMock;
    private readonly GetMaterialTypeList getMaterialTypeList;

    public GetMaterialTypeListTests()
    {
        this.mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        this.masterDataServiceClientMock = new Mock<IMasterDataServiceClient>();
        this.getMaterialTypeList = new GetMaterialTypeList(this.masterDataServiceClientMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkObjectResult()
    {
        // arrange
        var req = new DefaultHttpContext().Request;
        var cmsBaseArgDto = new CmsBaseArgDto();
        var materialTypes = new List<MaterialTypeDto>();
        this.mdsArgFactoryMock.Setup(s => s.CreateCmsCaseDataArgDto(It.IsAny<string>(), It.IsAny<Guid>())).Returns(cmsBaseArgDto);
        this.masterDataServiceClientMock
            .Setup(s => s.GetMaterialTypeListAsync(
                It.IsAny<CmsAuthValues>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(materialTypes);

        // act
        var result = await this.getMaterialTypeList.Run(req);

        // assert
        Assert.IsType<OkObjectResult>(result);
    }
}
