// <copyright file="GetWitnessStatementsLegacyTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions;

using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response;
using Common.Dto.Response.HouseKeeping;
using Cps.MasterDataService.Infrastructure.ApiClient;
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
    private readonly Mock<IMdsArgFactory> mdsArgFactoryMock;
    private readonly Mock<IMasterDataServiceClient> masterDataServiceClientMock;
    private readonly GetWitnessStatementsLegacy getWitnessStatements;

    public GetWitnessStatementsLegacyTests()
    {
        this.mdsArgFactoryMock = new Mock<IMdsArgFactory>();
        this.masterDataServiceClientMock = new Mock<IMasterDataServiceClient>();
        this.getWitnessStatements = new GetWitnessStatementsLegacy(this.mdsArgFactoryMock.Object, this.masterDataServiceClientMock.Object);
    }

    [Fact]
    public async Task Run_ShouldReturnOkObjectResult()
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
        var witnessStatementsResponse = new Cps.MasterDataService.Infrastructure.ApiClient.WitnessStatementResponseDto();
        this.mdsArgFactoryMock.Setup(s => s.CreateWitnessStatementsArgDto(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId, witnessId)).Returns(witnessStatementsArgDto);
        this.masterDataServiceClientMock
        .Setup(s => s.GetWitnessStatementsAsync(
            It.IsAny<GetWitnessStatementsRequest>(),
            It.IsAny<CmsAuthValues>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(witnessStatementsResponse);
        // act
        var result = await this.getWitnessStatements.Run(req, caseUrn, caseId, witnessId);

        // assert
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

        var witnessStatementsResponse = new Cps.MasterDataService.Infrastructure.ApiClient.WitnessStatementResponseDto()
        {
            StatementsForWitness =
            [
                new Cps.MasterDataService.Infrastructure.ApiClient.WitnessStatementDto
                {
                    Id = 100,
                },
                new Cps.MasterDataService.Infrastructure.ApiClient.WitnessStatementDto
                {
                    Id = 200,
                },
                new Cps.MasterDataService.Infrastructure.ApiClient.WitnessStatementDto
                {
                    Id = 0,
                },
            ],
        };

        this.mdsArgFactoryMock.Setup(s => s.CreateWitnessStatementsArgDto(It.IsAny<string>(), It.IsAny<Guid>(), caseUrn, caseId, witnessId)).Returns(witnessStatementsArgDto);
        this.masterDataServiceClientMock
        .Setup(s => s.GetWitnessStatementsAsync(
            It.IsAny<GetWitnessStatementsRequest>(),
            It.IsAny<CmsAuthValues>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(witnessStatementsResponse);

        // act
        var result = await this.getWitnessStatements.Run(req, caseUrn, caseId, witnessId);

        // assert
        // assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        var response = Assert.IsType<WitnessStatementResponseDto>(
            okResult.Value);

        Assert.Equal(3, response.StatementsForWitness.Count);

        Assert.Collection(
            response.StatementsForWitness,
            statement =>
            {
                Assert.Equal(100, statement.Id);
            },
            statement =>
            {
                Assert.Equal(200, statement.Id);
            },
            statement =>
            {
                Assert.Equal(0, statement.Id);
            });
    }
}
