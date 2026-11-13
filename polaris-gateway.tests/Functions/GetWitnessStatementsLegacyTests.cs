// <copyright file="GetWitnessStatementsLegacyTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions;

using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response;
using Common.Dto.Response.HouseKeeping;
using Ddei.Domain.CaseData.Args;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class GetWitnessStatementsLegacyTests
{
    private readonly Mock<IMdsArgFactory> _mdsArgFactoryMock;
    private readonly Mock<IMasterDataServiceClient> _masterDataServiceClientMock;
    private readonly GetWitnessStatementsLegacy _getWitnessStatements;

    public GetWitnessStatementsLegacyTests()
    {
        _mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        _masterDataServiceClientMock = new Mock<IMasterDataServiceClient>();
        _getWitnessStatements = new GetWitnessStatementsLegacy(_mdsArgFactoryMock.Object, _masterDataServiceClientMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkObjectResult()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var witnessId = 2;
        var witnessStatementsArgDto = new MdsWitnessStatementsArgDto
        {
            WitnessId = witnessId,
        };
        var witnessStatementsResponse = new WitnessStatementsResponse();
        _mdsArgFactoryMock.Setup(s => s.CreateWitnessStatementsArgDto(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId, witnessId)).Returns(witnessStatementsArgDto);
        _masterDataServiceClientMock
        .Setup(s => s.GetWitnessStatementsAsync(
            It.IsAny<GetWitnessStatementsRequest>(),
            It.IsAny<CmsAuthValues>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(witnessStatementsResponse);
        //act
        var result = await _getWitnessStatements.Run(req, caseUrn, caseId, witnessId);

        //assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Run_ShouldReturnWitnessStatementsWithParentIdMatchingDocumentId()
    {
        //arrange
        var req = new DefaultHttpContext().Request;
        var caseUrn = "caseUrn";
        var caseId = 1;
        var witnessId = 2;
        var witnessStatementsArgDto = new MdsWitnessStatementsArgDto
        {
            WitnessId = witnessId,
        };

        var witnessStatementsResponse = new WitnessStatementsResponse
        {
            WitnessStatements =
            [
                new WitnessStatement(100, 1),
                new WitnessStatement(200, 2),
                new WitnessStatement(0, 3),
            ],
        };

        _mdsArgFactoryMock.Setup(s => s.CreateWitnessStatementsArgDto(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId, witnessId)).Returns(witnessStatementsArgDto);
        _masterDataServiceClientMock
        .Setup(s => s.GetWitnessStatementsAsync(
            It.IsAny<GetWitnessStatementsRequest>(),
            It.IsAny<CmsAuthValues>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(witnessStatementsResponse);

        //act
        var result = await _getWitnessStatements.Run(req, caseUrn, caseId, witnessId);

        //assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<WitnessStatementsResponse>(okResult.Value);
        Assert.Equal(3, response.WitnessStatements.Count);
        Assert.Equal(100, response.WitnessStatements[0].Id);
        Assert.Equal(1, response.WitnessStatements[0].StatementNumber);

        Assert.Equal(200, response.WitnessStatements[1].Id);
        Assert.Equal(2, response.WitnessStatements[1].StatementNumber);

        Assert.Equal(0, response.WitnessStatements[2].Id);
        Assert.Equal(3, response.WitnessStatements[2].StatementNumber);
    }
}
