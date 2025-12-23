// <copyright file="BulkSetUnusedService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Cps.Fct.Hk.Ui.Interfaces;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using DdeiClient.Clients.Interfaces;
using Common.Dto.Request;
using Common.Constants;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;

/// <summary>
/// Provides services for setting materials related to a case to unused in bulk.
/// </summary>
public class BulkSetUnusedService( 
    ILogger<BulkSetUnusedService> logger,
    IMasterDataServiceClient apiClient)
    : IBulkSetUnusedService
{
    private readonly ILogger<BulkSetUnusedService> logger = logger;
    private readonly IMasterDataServiceClient apiClient = apiClient;

    /// <summary>
    /// Sanitizes the input subject string by validating that it contains only alphanumeric characters, spaces,
    /// hyphens, or underscores, and limits the length to a maximum of 200 characters.
    /// </summary>
    /// <param name="subject">The input subject string to be sanitized and validated.</param>
    /// <returns>The sanitized subject string if it passes validation.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the subject contains invalid characters or exceeds the maximum allowed length of 200 characters.
    /// </exception>
    public static string SanitizeSubject(string subject)
    {
        // Regex allowing alphanumeric characters, spaces, hyphens, underscores, parentheses, periods, colons and commas
        if (!Regex.IsMatch(subject, RegexExpressions.RenameMaterialSubjectRegex))
        {
            throw new ArgumentException("Invalid characters in subject.");
        }

        // Limit length of the subject to prevent excessive input
        if (subject.Length > 200)
        {
            throw new ArgumentException("Subject length exceeds maximum allowed characters.");
        }

        return subject;
    }

    /// <inheritdoc />
    public async Task<BulkSetUnusedResponse> BulkSetUnusedAsync(int caseId, CmsAuthValues cmsAuthValues, IReadOnlyCollection<BulkSetUnusedRequest> bulkSetUnusedRequests)
    {
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);
        var reclassifiedMaterials = new ConcurrentDictionary<int, ReclassifiedMaterial>();
        var failedMaterials = new ConcurrentDictionary<int, FailedMaterial>();

        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Bulk setting materials to unused for caseId [{caseIdString}] ...");

            string classification = "OTHER";
            int documentTypeId = 1200;
            bool used = false;

            // Use Parallel.ForEachAsync to limit the degree of parallelism to 4
            await Parallel.ForEachAsync(bulkSetUnusedRequests, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (bulkSetUnusedRequest, cancellationToken) =>
            {
                this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Processing materialId: {bulkSetUnusedRequest.materialId}, subject: {bulkSetUnusedRequest.subject}");

                var reclassifyCommunicationRequest = new ReclassifyCommunicationRequest(
                    Guid.NewGuid(),
                    classification,
                    bulkSetUnusedRequest.materialId,
                    documentTypeId,
                    used,
                    bulkSetUnusedRequest.subject);

                try
                {
                    ReclassificationResponse response = await this.apiClient.ReclassifyCommunicationAsync(reclassifyCommunicationRequest, cmsAuthValues).ConfigureAwait(false);

                    if (response.ReclassifyCommunication.Id.Equals(bulkSetUnusedRequest.materialId))
                    {
                        reclassifiedMaterials.TryAdd(response.ReclassifyCommunication.Id, new ReclassifiedMaterial
                        {
                            MaterialId = response.ReclassifyCommunication.Id,
                            Subject = reclassifyCommunicationRequest.subject,
                        });

                        this.logger.LogInformation(LoggingConstants.BulkSetUnusedOperationSuccess, LoggingConstants.HskUiLogPrefix, caseId, bulkSetUnusedRequest.materialId);
                    }
                    else
                    {
                        throw new Exception($"{LoggingConstants.HskUiLogPrefix} Failed to reclassify material.");
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, LoggingConstants.BulkSetUnusedOperationFailed, LoggingConstants.HskUiLogPrefix, bulkSetUnusedRequest.materialId);
                    failedMaterials.TryAdd(bulkSetUnusedRequest.materialId, new FailedMaterial
                    {
                        MaterialId = bulkSetUnusedRequest.materialId,
                        Subject = bulkSetUnusedRequest.subject,
                        ErrorMessage = ex.Message,
                    });
                }
            }).ConfigureAwait(false);

            string status = this.DetermineStatus(failedMaterials, reclassifiedMaterials);

            return new BulkSetUnusedResponse
            {
                Status = status,
                Message = this.GenerateMessage(failedMaterials, reclassifiedMaterials),
                ReclassifiedMaterials = reclassifiedMaterials.Values.ToList(),
                FailedMaterials = failedMaterials.Values.ToList(),
            };
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} DDEI-EAS API Error occurred while bulk setting materials to unused for caseId [{caseIdString}]");
            throw;
        }
    }

    /// <summary>
    /// Determines the status of the bulk set unused operation based on the failed and reclassified materials.
    /// </summary>
    /// <param name="failedMaterials">The collection of failed materials.</param>
    /// <param name="reclassifiedMaterials">The collection of successfully reclassified materials.</param>
    /// <returns>A string representing the status of the operation: "success", "failed", "partial_success", or "unassigned".</returns>
    private string DetermineStatus(ConcurrentDictionary<int, FailedMaterial> failedMaterials, ConcurrentDictionary<int, ReclassifiedMaterial> reclassifiedMaterials)
    {
        if (!failedMaterials.IsEmpty && !reclassifiedMaterials.IsEmpty)
        {
            return "partial_success";
        }

        if (!failedMaterials.IsEmpty && reclassifiedMaterials.IsEmpty)
        {
            return "failed";
        }

        return failedMaterials.IsEmpty && !reclassifiedMaterials.IsEmpty ? "success" : "unassigned";
    }

    /// <summary>
    /// Generates a message summarizing the results of the bulk set unused operation.
    /// </summary>
    /// <param name="failedMaterials">The collection of failed materials.</param>
    /// <param name="reclassifiedMaterials">The collection of successfully reclassified materials.</param>
    /// <returns>A string message summarizing the success or failure of the operation.</returns>
    private string GenerateMessage(ConcurrentDictionary<int, FailedMaterial> failedMaterials, ConcurrentDictionary<int, ReclassifiedMaterial> reclassifiedMaterials)
    {
        if (failedMaterials.IsEmpty)
        {
            return "All materials were successfully reclassified.";
        }

        return reclassifiedMaterials.IsEmpty ? "No materials were reclassified due to errors."
                : "Some materials were successfully reclassified, but errors occurred for others.";
    }
}
