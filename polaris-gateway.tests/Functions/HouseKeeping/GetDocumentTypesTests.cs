// <copyright file="GetDocumentTypesTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using System;
using System.Collections.Generic;
using Common.Constants;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions.HouseKeeping;
using Xunit;

/// <summary>
/// Tests for GetDocumentTypes fucntion.
/// </summary>
public class GetDocumentTypesTests
{
    private readonly TestLogger<GetDocumentTypes> mockLogger;
    private readonly Mock<IDocumentTypeMapper> mockDocumentTypeMapper;
    private readonly GetDocumentTypes sutGetDocumentTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDocumentTypesTests"/> class.
    /// </summary>
    public GetDocumentTypesTests()
    {
        mockLogger = new TestLogger<GetDocumentTypes>();
        mockDocumentTypeMapper = new Mock<IDocumentTypeMapper>();

        sutGetDocumentTypes = new GetDocumentTypes(mockLogger, mockDocumentTypeMapper.Object);
    }

    /// <summary>
    /// Tests that the function returns an unauthorized error when unauthorized exception is thrown.
    /// </summary>
    [Fact]
    public void Run_ReturnsUnauthorizedError_WhenUnauthorizedAccessExceptionIsThrown()
    {
        // Arrange
        Mock<HttpRequest> mockRequest = SetUpMockRequest();

        mockDocumentTypeMapper
             .Setup(svc => svc.GetDocumentTypesWithClassificationGroup())
            .Throws(new UnauthorizedAccessException("Unauthorized"));

        // Act
        IActionResult result = sutGetDocumentTypes.Run(mockRequest.Object, 123);

        // Assert
        UnauthorizedObjectResult unauthorizedAccessResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedAccessResult.StatusCode);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetDocumentTypes function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetDocumentTypes function encountered an unauthorized access error: Unauthorized"));
    }

    /// <summary>
    /// Tests that the function returns an unauthorized error when unauthorized exception is thrown.
    /// </summary>
    [Fact]
    public void Run_ReturnsOkResponse_WhenRequestIsProcessedSuccessfully()
    {
        // Arrange
        Mock<HttpRequest> mockRequest = SetUpMockRequest();

        var response = new List<DocumentTypeGroup>()
        {
            new () { Id = 1, Name = "MG101", Group = "MG Forms", Category = "Communication" },
        };

        mockDocumentTypeMapper
           .Setup(svc => svc.GetDocumentTypesWithClassificationGroup())
          .Returns(response);

        // Act
        IActionResult result = sutGetDocumentTypes.Run(mockRequest.Object, 123);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        Assert.Equal(response, (IReadOnlyList<DocumentTypeGroup>)okResult.Value);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetDocumentTypes function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] GetDocumentTypes function completed"));

        mockDocumentTypeMapper.Verify(svc => svc.GetDocumentTypesWithClassificationGroup(), Times.Once);
    }

    private static Mock<HttpRequest> SetUpMockRequest()
    {
        var mockRequest = new Mock<HttpRequest>();

        // Set up a DefaultHttpContext to support setting headers
        var context = new DefaultHttpContext();
        mockRequest.Setup(r => r.HttpContext).Returns(context);
        mockRequest.Setup(r => r.Headers.Add("corelation", "1232131231"));

        return mockRequest;
    }
}
