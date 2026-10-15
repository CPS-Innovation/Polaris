// <copyright file="GetExhibitProducersLegacyTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions;

using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.Case;
using Common.Dto.Response.HouseKeeping;
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

public class GetExhibitProducersLegacyTests
{
    private readonly Mock<IMasterDataServiceClient> _masterDataServiceClientMock;
    private readonly Mock<IMdsArgFactory> _mdsArgFactoryMock;
    private readonly GetExhibitProducersLegacy _getExhibitProducers;

    public GetExhibitProducersLegacyTests()
    {
        _masterDataServiceClientMock = new Mock<IMasterDataServiceClient>();
        _mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        _getExhibitProducers = new GetExhibitProducersLegacy(_masterDataServiceClientMock.Object, _mdsArgFactoryMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkObjectResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto();
        var exhibitProducers = new List<Common.Dto.Response.HouseKeeping.ExhibitProducer>();
        _mdsArgFactoryMock.Setup(s => s.CreateCaseIdentifiersArg(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId)).Returns(mdsCaseIdentifiersArgDto);
        var response = new ExhibitProducersResponse
        {
            ExhibitProducers = exhibitProducers,
        };

        _masterDataServiceClientMock
            .Setup(s => s.GetExhibitProducersAsync(
                It.IsAny<GetExhibitProducersRequest>(),
                It.IsAny<CmsAuthValues>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        //act
        var result = await _getExhibitProducers.Run(req, caseUrn, caseId);

        //assert
        Assert.IsType<OkObjectResult>(result);
    }
}