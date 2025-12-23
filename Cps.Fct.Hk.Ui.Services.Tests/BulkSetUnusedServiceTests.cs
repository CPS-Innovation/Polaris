// <copyright file="BulkSetUnusedServiceTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Tests;

using Microsoft.Extensions.Logging;
using Moq;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using System.Collections.Concurrent;
using FluentAssertions;
using DdeiClient.Clients.Interfaces;
using Xunit;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Request;
using Common.Dto.Response.HouseKeeping;
using Common.Constants;

/// <summary>
/// Unit tests for the BulkSetUnusedService class.
/// </summary>
public class BulkSetUnusedServiceTests
{
    private readonly TestLogger<BulkSetUnusedService> mockLogger;
    private readonly Mock<IMasterDataServiceClient> mockApiClient;
    private readonly BulkSetUnusedService bulkSetUnusedService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkSetUnusedServiceTests"/> class.
    /// </summary>
    public BulkSetUnusedServiceTests()
    {
        this.mockLogger = new TestLogger<BulkSetUnusedService>();
        this.mockApiClient = new Mock<IMasterDataServiceClient>();
        this.bulkSetUnusedService = new BulkSetUnusedService(this.mockLogger, this.mockApiClient.Object);
    }

#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

    /// <summary>
    /// Tests that all materials are successfully reclassified when calling BulkSetUnusedAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task BulkSetUnusedAsync_AllMaterialsReclassified_ReturnsSuccess()
    {
        // Arrange
        var bulkSetUnusedRequests = new List<BulkSetUnusedRequest>
        {
            new BulkSetUnusedRequest { materialId = 1, subject = "Material 1" },
            new BulkSetUnusedRequest { materialId = 2, subject = "Material 2" },
        };

        // var cmsAuthValues = new CmsAuthValues();
        var cmsAuthValues = new CmsAuthValues("cookies", "token", Guid.NewGuid());

        this.mockApiClient.Setup(client => client.ReclassifyCommunicationAsync(It.IsAny<ReclassifyCommunicationRequest>(), It.IsAny<CmsAuthValues>()))
                         .ReturnsAsync((ReclassifyCommunicationRequest req, CmsAuthValues _) =>
                             new ReclassificationResponse(
                                 new ReclassifyCommunication { Id = req.materialId }));

        // Act
        BulkSetUnusedResponse result = await this.bulkSetUnusedService.BulkSetUnusedAsync(123, cmsAuthValues, bulkSetUnusedRequests);

        // Assert
        Assert.Equal("success", result.Status);
        Assert.Equal(2, result.ReclassifiedMaterials.Count);
        Assert.Empty(result.FailedMaterials);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Bulk setting materials to unused for caseId [123] ..."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [123] successfully reclassified materialId"));
    }

    /// <summary>
    /// Tests that partial success is returned when some materials fail reclassification in BulkSetUnusedAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task BulkSetUnusedAsync_PartialSuccess_ReturnsPartialSuccess()
    {
        // Arrange
        var bulkSetUnusedRequests = new List<BulkSetUnusedRequest>
        {
            new BulkSetUnusedRequest { materialId = 1, subject = "Material 1" },
            new BulkSetUnusedRequest { materialId = 2, subject = "Material 2" },
        };

        var cmsAuthValues = new CmsAuthValues("cookies", "token", Guid.NewGuid());

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        this.mockApiClient.Setup(client => client.ReclassifyCommunicationAsync(It.IsAny<ReclassifyCommunicationRequest>(), cmsAuthValues))
            .ReturnsAsync((ReclassifyCommunicationRequest request, CmsAuthValues authValues) =>
            {
                if (request.materialId == 1)
                {
                    // Return a valid ReclassificationResponse for materialId 1
                    return new ReclassificationResponse(new ReclassifyCommunication { Id = 1 });
                }
                else if (request.materialId == 2)
                {
                    // Simulate failure by throwing an exception
                    throw new Exception("Reclassification failed for Material 2");
                }

                // Explicitly return null for any other cases (nullable type handling)
                return (ReclassificationResponse?)null;
            });
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

        // Act
        BulkSetUnusedResponse result = await this.bulkSetUnusedService.BulkSetUnusedAsync(123, cmsAuthValues, bulkSetUnusedRequests).ConfigureAwait(true);

        // Assert
        Assert.Equal("partial_success", result.Status);

        // Assert ReclassifiedMaterials
        Assert.Single(result.ReclassifiedMaterials);
        Assert.Contains(result.ReclassifiedMaterials, material => material.MaterialId == 1);

        // Assert FailedMaterials
        Assert.Single(result.FailedMaterials);
        Assert.Contains(result.FailedMaterials, failed => failed.MaterialId == 2 && failed.ErrorMessage == "Reclassification failed for Material 2");

        // Assert that logging occurred as expected
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Bulk setting materials to unused for caseId [123] ..."));
    }

    /// <summary>
    /// Tests that when all materials fail to be reclassified in BulkSetUnusedAsync, the method returns a "failed" status.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task BulkSetUnusedAsync_AllMaterialsFail_ReturnsFailed()
    {
        // Arrange
        var bulkSetUnusedRequests = new List<BulkSetUnusedRequest>
        {
            new BulkSetUnusedRequest { materialId = 1, subject = "Material 1" },
            new BulkSetUnusedRequest { materialId = 2, subject = "Material 2" },
        };

        var cmsAuthValues = new CmsAuthValues("cookies", "token", Guid.NewGuid());

        this.mockApiClient.Setup(client => client.ReclassifyCommunicationAsync(It.IsAny<ReclassifyCommunicationRequest>(), It.IsAny<CmsAuthValues>()))
                     .ThrowsAsync(new Exception("Reclassification failed"));

        // Act
        BulkSetUnusedResponse result = await this.bulkSetUnusedService.BulkSetUnusedAsync(123, cmsAuthValues, bulkSetUnusedRequests);

        // Assert
        Assert.Equal("failed", result.Status);
        Assert.Empty(result.ReclassifiedMaterials);
        Assert.Equal(2, result.FailedMaterials.Count);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Bulk setting materials to unused for caseId [123] ..."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Failed to reclassify materialId"));
    }

    /// <summary>
    /// Tests that when an API exception is thrown during the call to ReclassifyCommunicationAsync,
    /// BulkSetUnusedAsync catches the exception and returns a failed status.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task BulkSetUnusedAsync_ApiThrowsException_ThrowsException()
    {
        // Arrange
        var exception = new Exception("API Error");
        var bulkSetUnusedRequests = new List<BulkSetUnusedRequest>
        {
            new BulkSetUnusedRequest { materialId = 1, subject = "Material 1" },
        };

        var cmsAuthValues = new CmsAuthValues("cookies", "token", Guid.NewGuid());

        this.mockApiClient.Setup(client => client.ReclassifyCommunicationAsync(It.IsAny<ReclassifyCommunicationRequest>(), cmsAuthValues))
            .ThrowsAsync(exception);

        // Act
        BulkSetUnusedResponse result = await this.bulkSetUnusedService.BulkSetUnusedAsync(123, cmsAuthValues, bulkSetUnusedRequests);

        // Assert
        Assert.Equal("failed", result.Status);
        Assert.Empty(result.ReclassifiedMaterials);
        Assert.Single(result.FailedMaterials);

        FailedMaterial failedMaterial = result.FailedMaterials.First();
        Assert.Equal("API Error", failedMaterial.ErrorMessage);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Bulk setting materials to unused for caseId [123] ..."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Failed to reclassify materialId"));
    }

    /// <summary>
    /// Tests that when a material fails reclassification in BulkSetUnusedAsync,
    /// the failure is logged as an error.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task BulkSetUnusedAsync_LogsErrorWhenMaterialFails()
    {
        // Arrange
        var bulkSetUnusedRequests = new List<BulkSetUnusedRequest>
        {
            new BulkSetUnusedRequest { materialId = 1, subject = "Material 1" },
        };

        var cmsAuthValues = new CmsAuthValues("cookies", "token", Guid.NewGuid());

        this.mockApiClient.Setup(client => client.ReclassifyCommunicationAsync(It.IsAny<ReclassifyCommunicationRequest>(), It.IsAny<CmsAuthValues>()))
                     .ThrowsAsync(new Exception("Reclassification failed"));

        // Act
        await this.bulkSetUnusedService.BulkSetUnusedAsync(123, cmsAuthValues, bulkSetUnusedRequests);

        // Assert
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Bulk setting materials to unused for caseId [123] ..."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Failed to reclassify materialId [1]"));
    }

    /// <summary>
    /// Tests the status determination when both failed and reclassified materials are present.
    /// </summary>
    [Fact]
    public void DetermineStatus_WhenFailedMaterialsAndReclassifiedMaterialsAreNotEmpty_ReturnsPartialSuccess()
    {
        // Arrange
        var failedMaterials = new ConcurrentDictionary<int, FailedMaterial>
        {
            [1] = new FailedMaterial { MaterialId = 1, ErrorMessage = "Error 1" },
        };
        var reclassifiedMaterials = new ConcurrentDictionary<int, ReclassifiedMaterial>
        {
            [2] = new ReclassifiedMaterial { MaterialId = 2, Subject = "Material 2" },
        };

        // Act
        string result = this.InvokeDetermineStatus(failedMaterials, reclassifiedMaterials);

        // Assert
        Assert.Equal("partial_success", result);
    }

    /// <summary>
    /// Tests the status determination when only failed materials are present.
    /// </summary>
    [Fact]
    public void DetermineStatus_WhenOnlyFailedMaterialsAreNotEmpty_ReturnsFailed()
    {
        // Arrange
        var failedMaterials = new ConcurrentDictionary<int, FailedMaterial>
        {
            [1] = new FailedMaterial { MaterialId = 1, ErrorMessage = "Error 1" },
        };
        var reclassifiedMaterials = new ConcurrentDictionary<int, ReclassifiedMaterial>();

        // Act
        string result = this.InvokeDetermineStatus(failedMaterials, reclassifiedMaterials);

        // Assert
        Assert.Equal("failed", result);
    }

    /// <summary>
    /// Tests the status determination when only reclassified materials are present.
    /// </summary>
    [Fact]
    public void DetermineStatus_WhenOnlyReclassifiedMaterialsAreNotEmpty_ReturnsSuccess()
    {
        // Arrange
        var failedMaterials = new ConcurrentDictionary<int, FailedMaterial>();
        var reclassifiedMaterials = new ConcurrentDictionary<int, ReclassifiedMaterial>
        {
            [2] = new ReclassifiedMaterial { MaterialId = 2, Subject = "Material 2" },
        };

        // Act
        string result = this.InvokeDetermineStatus(failedMaterials, reclassifiedMaterials);

        // Assert
        Assert.Equal("success", result);
    }

    /// <summary>
    /// Tests the status determination when both collections are empty.
    /// </summary>
    [Fact]
    public void DetermineStatus_WhenBothAreEmpty_ReturnsUnassigned()
    {
        // Arrange
        var failedMaterials = new ConcurrentDictionary<int, FailedMaterial>();
        var reclassifiedMaterials = new ConcurrentDictionary<int, ReclassifiedMaterial>();

        // Act
        string result = this.InvokeDetermineStatus(failedMaterials, reclassifiedMaterials);

        // Assert
        Assert.Equal("unassigned", result);
    }

    /// <summary>
    /// Tests the message generation when there are no failed materials.
    /// </summary>
    [Fact]
    public void GenerateMessage_WhenNoFailedMaterials_ReturnsSuccessMessage()
    {
        // Arrange
        var failedMaterials = new ConcurrentDictionary<int, FailedMaterial>();
        var reclassifiedMaterials = new ConcurrentDictionary<int, ReclassifiedMaterial>
        {
            [1] = new ReclassifiedMaterial { MaterialId = 1, Subject = "Material 1" },
        };

        // Act
        string result = this.InvokeGenerateMessage(failedMaterials, reclassifiedMaterials);

        // Assert
        Assert.Equal("All materials were successfully reclassified.", result);
    }

    /// <summary>
    /// Tests the message generation when only failed materials are present.
    /// </summary>
    [Fact]
    public void GenerateMessage_WhenOnlyFailedMaterials_ReturnsErrorMessage()
    {
        // Arrange
        var failedMaterials = new ConcurrentDictionary<int, FailedMaterial>
        {
            [1] = new FailedMaterial { MaterialId = 1, ErrorMessage = "Error 1" },
        };
        var reclassifiedMaterials = new ConcurrentDictionary<int, ReclassifiedMaterial>();

        // Act
        string result = this.InvokeGenerateMessage(failedMaterials, reclassifiedMaterials);

        // Assert
        Assert.Equal("No materials were reclassified due to errors.", result);
    }

    /// <summary>
    /// Tests the message generation when some materials are reclassified but errors occurred.
    /// </summary>
    [Fact]
    public void GenerateMessage_WhenSomeMaterialsReclassifiedButErrorsOccurred_ReturnsPartialMessage()
    {
        // Arrange
        var failedMaterials = new ConcurrentDictionary<int, FailedMaterial>
        {
            [1] = new FailedMaterial { MaterialId = 1, ErrorMessage = "Error 1" },
        };
        var reclassifiedMaterials = new ConcurrentDictionary<int, ReclassifiedMaterial>
        {
            [2] = new ReclassifiedMaterial { MaterialId = 2, Subject = "Material 2" },
        };

        // Act
        string result = this.InvokeGenerateMessage(failedMaterials, reclassifiedMaterials);

        // Assert
        Assert.Equal("Some materials were successfully reclassified, but errors occurred for others.", result);
    }

    /// <summary>
    /// Tests that the SanitizeSubject method returns the same string when given valid input.
    /// </summary>
    [Fact]
    public void SanitizeSubject_ValidInput_ReturnsSameString()
    {
        // Arrange
        string validSubject = "Valid_Subject-123";

        // Act
        string result = BulkSetUnusedService.SanitizeSubject(validSubject);

        // Assert
        result.Should().Be(validSubject);
    }

    /// <summary>
    /// Tests that the SanitizeSubject method throws an ArgumentException for invalid characters.
    /// </summary>
    [Fact]
    public void SanitizeSubject_InvalidCharacters_ThrowsArgumentException()
    {
        // Arrange
        string invalidSubject = "Invalid!@#";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => BulkSetUnusedService.SanitizeSubject(invalidSubject));
    }

    /// <summary>
    /// Tests that the SanitizeSubject method throws an ArgumentException for input that exceeds the length limit.
    /// </summary>
    [Fact]
    public void SanitizeSubject_ExceedsLengthLimit_ThrowsArgumentException()
    {
        // Arrange
        string longSubject = new string('a', 201); // Exceeds the 200 character limit

        // Act & Assert
        Assert.Throws<ArgumentException>(() => BulkSetUnusedService.SanitizeSubject(longSubject));
    }

    /// <summary>
    /// Invokes the private method to determine the status of the bulk set unused operation.
    /// </summary>
    /// <param name="failedMaterials">The collection of failed materials.</param>
    /// <param name="reclassifiedMaterials">The collection of successfully reclassified materials.</param>
    /// <returns>The status of the operation as a string.</returns>
    private string InvokeDetermineStatus(ConcurrentDictionary<int, FailedMaterial> failedMaterials, ConcurrentDictionary<int, ReclassifiedMaterial> reclassifiedMaterials)
    {
        // Attempt to get the MethodInfo for the DetermineStatus method
        System.Reflection.MethodInfo? methodInfo = typeof(BulkSetUnusedService).GetMethod("DetermineStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Check if methodInfo is null
        if (methodInfo == null)
        {
            throw new InvalidOperationException("Could not find the method 'DetermineStatus' in BulkSetUnusedService.");
        }

        // Invoke the method and cast the result to string
        return (string)methodInfo.Invoke(this.bulkSetUnusedService, new object[] { failedMaterials, reclassifiedMaterials });
    }

    /// <summary>
    /// Invokes the private method to generate a message summarizing the results of the bulk set unused operation.
    /// </summary>
    /// <param name="failedMaterials">The collection of failed materials.</param>
    /// <param name="reclassifiedMaterials">The collection of successfully reclassified materials.</param>
    /// <returns>A summary message of the operation.</returns>
    private string InvokeGenerateMessage(ConcurrentDictionary<int, FailedMaterial> failedMaterials, ConcurrentDictionary<int, ReclassifiedMaterial> reclassifiedMaterials)
    {
        // Attempt to get the MethodInfo for the GenerateMessage method
        System.Reflection.MethodInfo? methodInfo = typeof(BulkSetUnusedService).GetMethod("GenerateMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Check if methodInfo is null
        if (methodInfo == null)
        {
            throw new InvalidOperationException("Could not find the method 'GenerateMessage' in BulkSetUnusedService.");
        }

        // Invoke the method and cast the result to string
        return (string)methodInfo.Invoke(this.bulkSetUnusedService, new object[] { failedMaterials, reclassifiedMaterials });
    }

#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
}
