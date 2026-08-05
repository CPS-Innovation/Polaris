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
    /// Tests that the function returns OK response when the request is processed successfully.
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

    /// <summary>
    /// Tests that the function returns OK response with empty list when document type mapper returns empty list.
    /// </summary>
    [Fact]
    public void Run_ReturnsOkResponseWithEmptyList_WhenDocumentTypeMapperReturnsEmptyList()
    {
        // Arrange
        Mock<HttpRequest> mockRequest = SetUpMockRequest();

        var emptyResponse = new List<DocumentTypeGroup>();

        mockDocumentTypeMapper
           .Setup(svc => svc.GetDocumentTypesWithClassificationGroup())
          .Returns(emptyResponse);

        // Act
        IActionResult result = sutGetDocumentTypes.Run(mockRequest.Object, 123);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var resultList = Assert.IsType<List<DocumentTypeGroup>>(okResult.Value);
        Assert.Empty(resultList);

        mockDocumentTypeMapper.Verify(svc => svc.GetDocumentTypesWithClassificationGroup(), Times.Once);
    }

    /// <summary>
    /// Tests that the function returns OK response with multiple document types grouped correctly.
    /// </summary>
    [Fact]
    public void Run_ReturnsOkResponseWithMultipleGroups_WhenDocumentTypeMapperReturnsMultipleGroups()
    {
        // Arrange
        Mock<HttpRequest> mockRequest = SetUpMockRequest();

        var response = new List<DocumentTypeGroup>()
        {
            new () { Id = 1031, Name = "MG11", Group = "Statement", Category = "Statement" },
            new () { Id = 1030, Name = "Other Exhibit", Group = "Exhibit", Category = "Exhibit" },
            new () { Id = 1066, Name = "MG00", Group = "MG Form", Category = "MG Form" },
            new () { Id = 1200, Name = "Other Material", Group = "Other", Category = "Other Material" },
        };

        mockDocumentTypeMapper
           .Setup(svc => svc.GetDocumentTypesWithClassificationGroup())
          .Returns(response);

        // Act
        IActionResult result = sutGetDocumentTypes.Run(mockRequest.Object, 456);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var resultList = (IReadOnlyList<DocumentTypeGroup>)okResult.Value;
        Assert.Equal(4, resultList.Count);
        Assert.Contains(resultList, dt => dt.Group == "Statement");
        Assert.Contains(resultList, dt => dt.Group == "Exhibit");
        Assert.Contains(resultList, dt => dt.Group == "MG Form");
        Assert.Contains(resultList, dt => dt.Group == "Other");

        mockDocumentTypeMapper.Verify(svc => svc.GetDocumentTypesWithClassificationGroup(), Times.Once);
    }

    /// <summary>
    /// Tests that the function returns bad request when invalid case ID is provided.
    /// </summary>
    [Fact]
    public void Run_ReturnsBadRequest_WhenInvalidCaseIdIsProvided()
    {
        // Arrange
        Mock<HttpRequest> mockRequest = SetUpMockRequest();

        // Act
        IActionResult result = sutGetDocumentTypes.Run(mockRequest.Object, 0);

        // Assert
        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        Assert.Contains("Invalid case Id", badRequestResult.Value?.ToString());

        mockDocumentTypeMapper.Verify(svc => svc.GetDocumentTypesWithClassificationGroup(), Times.Never);
    }

    /// <summary>
    /// Tests that the function returns bad request when negative case ID is provided.
    /// </summary>
    [Fact]
    public void Run_ReturnsBadRequest_WhenNegativeCaseIdIsProvided()
    {
        // Arrange
        Mock<HttpRequest> mockRequest = SetUpMockRequest();

        // Act
        IActionResult result = sutGetDocumentTypes.Run(mockRequest.Object, -1);

        // Assert
        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

        mockDocumentTypeMapper.Verify(svc => svc.GetDocumentTypesWithClassificationGroup(), Times.Never);
    }

    /// <summary>
    /// Tests that the function returns unprocessable entity when invalid operation exception is thrown by document type mapper.
    /// </summary>
    [Fact]
    public void Run_ReturnsUnprocessableEntity_WhenInvalidOperationExceptionIsThrown()
    {
        // Arrange
        Mock<HttpRequest> mockRequest = SetUpMockRequest();

        mockDocumentTypeMapper
             .Setup(svc => svc.GetDocumentTypesWithClassificationGroup())
            .Throws(new InvalidOperationException("Invalid operation"));

        // Act
        IActionResult result = sutGetDocumentTypes.Run(mockRequest.Object, 123);

        // Assert
        UnprocessableEntityObjectResult unprocessableResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, unprocessableResult.StatusCode);
        Assert.Contains("Invalid operation", unprocessableResult.Value?.ToString());

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetDocumentTypes function encountered an invalid operation error"));
    }

    /// <summary>
    /// Tests that the function returns unprocessable entity when not supported exception is thrown by document type mapper.
    /// </summary>
    [Fact]
    public void Run_ReturnsUnprocessableEntity_WhenNotSupportedExceptionIsThrown()
    {
        // Arrange
        Mock<HttpRequest> mockRequest = SetUpMockRequest();

        mockDocumentTypeMapper
             .Setup(svc => svc.GetDocumentTypesWithClassificationGroup())
            .Throws(new NotSupportedException("Not supported"));

        // Act
        IActionResult result = sutGetDocumentTypes.Run(mockRequest.Object, 123);

        // Assert
        UnprocessableEntityObjectResult unprocessableResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, unprocessableResult.StatusCode);
        Assert.Contains("GetDocumentTypes error: Not supported", unprocessableResult.Value?.ToString());

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetDocumentTypes function encountered an unsupported content type error"));
    }

    /// <summary>
    /// Tests that the function returns internal server error when generic exception is thrown by document type mapper.
    /// </summary>
    [Fact]
    public void Run_ReturnsInternalServerError_WhenGenericExceptionIsThrown()
    {
        // Arrange
        Mock<HttpRequest> mockRequest = SetUpMockRequest();

        mockDocumentTypeMapper
             .Setup(svc => svc.GetDocumentTypesWithClassificationGroup())
            .Throws(new Exception("Unexpected error"));

        // Act
        IActionResult result = sutGetDocumentTypes.Run(mockRequest.Object, 123);

        // Assert
        StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetDocumentTypes function encountered an error"));
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
