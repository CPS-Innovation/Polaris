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
        // arrange
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
                new WitnessStatement(100, 1, 10),
                new WitnessStatement(200, 2, 20),
                new WitnessStatement(0, 3, null),
            ],
        };

        _mdsArgFactoryMock
            .Setup(s => s.CreateWitnessStatementsArgDto(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                caseUrn,
                caseId,
                witnessId))
            .Returns(witnessStatementsArgDto);

        _masterDataServiceClientMock
            .Setup(s => s.GetWitnessStatementsAsync(
                It.IsAny<GetWitnessStatementsRequest>(),
                It.IsAny<CmsAuthValues>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(witnessStatementsResponse);

        // act
        var result = await _getWitnessStatements.Run(req,caseUrn, caseId, witnessId);

        // assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        var response =
            Assert.IsType<List<WitnessStatementDto>>(okResult.Value);

        Assert.Equal(3, response.Count);

        Assert.Equal(10, response[0].DocumentId);
        Assert.Equal(10, response[0].ParentId);
        Assert.Equal(1, response[0].StatementNumber);

        Assert.Equal(20, response[1].DocumentId);
        Assert.Equal(20, response[1].ParentId);
        Assert.Equal(2, response[1].StatementNumber);

        Assert.Null(response[2].DocumentId);
        Assert.Null(response[2].ParentId);
        Assert.Equal(3, response[2].StatementNumber);
    }
}
