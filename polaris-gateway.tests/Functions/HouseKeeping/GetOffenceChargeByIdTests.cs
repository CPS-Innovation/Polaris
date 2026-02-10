// <copyright file="GetOffenceChargeByIdTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Dto.Request;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolarisGateway.Functions.HouseKeeping;
using Xunit;

using ApiClient = Cps.MasterDataService.Infrastructure.ApiClient;

public class GetOffenceChargeByIdTests
{
    private readonly TestLogger<GetOffenceChargeById> mockLogger;
    private readonly Mock<ICommunicationService> mockCommunicationService;
    private readonly GetOffenceChargeById function;

    public GetOffenceChargeByIdTests()
    {
        mockLogger = new TestLogger<GetOffenceChargeById>();
        mockCommunicationService = new Mock<ICommunicationService>();
        
        function = new GetOffenceChargeById(
            mockLogger,
            mockCommunicationService.Object);
    }

    private Mock<HttpRequest> CreateRequest()
    {
        var req = new Mock<HttpRequest>();
        req.Setup(r => r.HttpContext).Returns(new DefaultHttpContext());
        req.Setup(r => r.Headers.Add("corelation", "1232131231"));
        return req;
    }

    [Fact]
    public async Task Run_ReturnsUnprocessableEntity_WhenServiceReturnsNull()
    {
        var request = CreateRequest();

        mockCommunicationService
            .Setup(x => x.GetOffenceChargeByHistoryIdAsync(100, 200, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync((ApiClient.OffenceChangeResponse)null);

        IActionResult result = await function.Run(request.Object, 100, 200);

        var unprocessable = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Null(unprocessable.Value);
    }

    // ---------------------------------------------------------------------
    // HAPPY PATH → returns OK(result)
    // ---------------------------------------------------------------------
    [Fact]
    public async Task Run_ReturnsOk_WhenValidRequestAndDataFound()
    {
        var request = CreateRequest();

        var responseObj = new ApiClient.OffenceChangeResponse
        {
            CaseId = 123,
            Urn = "UISs23333",
            OffenceChanges = new List<ApiClient.OffenceChange>
            {
                new ApiClient.OffenceChange
                {
                    Code = "Test",
                },
            }
        };
           mockCommunicationService
            .Setup(x => x.GetOffenceChargeByHistoryIdAsync(100, 200, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(responseObj);

        IActionResult result = await function.Run(request.Object, 100, 200);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(responseObj, ok.Value);
    }

    // ---------------------------------------------------------------------
    // INVALID OPERATION → returns 422
    // ---------------------------------------------------------------------
    [Fact]
    public async Task Run_ReturnsUnprocessableEntity_WhenInvalidOperationThrown()
    {
        var request = CreateRequest();

        IActionResult result = await function.Run(request.Object, 100, 200);

        var unprocessable = Assert.IsType<UnprocessableEntityObjectResult>(result);
    }

    // ---------------------------------------------------------------------
    // NotSupportedException → 422
    // ---------------------------------------------------------------------
    [Fact]
    public async Task Run_ReturnsUnprocessableEntity_WhenNotSupportedExceptionThrown()
    {
        var request = CreateRequest();

        mockCommunicationService
            .Setup(x => x.GetOffenceChargeByHistoryIdAsync(100, 200, It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new NotSupportedException("Not supported"));

        IActionResult result = await function.Run(request.Object, 100, 200);

        var unprocessable = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Contains("Not supported", unprocessable.Value!.ToString());
    }

    // ---------------------------------------------------------------------
    // UnauthorizedAccessException → 401
    // ---------------------------------------------------------------------
    [Fact]
    public async Task Run_ReturnsUnauthorized_WhenUnauthorizedAccessThrows()
    {
        var request = CreateRequest();

        mockCommunicationService
            .Setup(x => x.GetOffenceChargeByHistoryIdAsync(100, 200, It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new UnauthorizedAccessException("unauthorized"));

        IActionResult result = await function.Run(request.Object, 100, 200);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    // ---------------------------------------------------------------------
    // Any other exception → 500
    // ---------------------------------------------------------------------
    [Fact]
    public async Task Run_Returns500_WhenUnexpectedException()
    {
        var request = CreateRequest();

        mockCommunicationService
            .Setup(x => x.GetOffenceChargeByHistoryIdAsync(100, 200, It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new Exception("Boom"));

        IActionResult result = await function.Run(request.Object, 100, 200);

        var status = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, status.StatusCode);
    }
}
