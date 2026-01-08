// <copyright file="UmaReclassify.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions.HouseKeeping;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Helpers;

/// <summary>
/// Represents a function that automatically sets materials to unused,
/// intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UmaReclassify"/> class.
/// </remarks>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="communicationService">The service used to process the retrieve communications request and generate the result.</param>
/// <param name="umaReclassifyService">The service used to process the communication matching request and generate the result.</param>
/// <param name="bulkSetUnusedService">The service used to process the reclassification request and generate the result.</param>
public class UmaReclassify(
    ILogger<UmaReclassify> logger,
    ICommunicationService communicationService,
    IUmaReclassifyService umaReclassifyService,
    IBulkSetUnusedService bulkSetUnusedService) : BaseFunction(logger)
{
    private readonly ILogger<UmaReclassify> logger = logger;
    private readonly ICommunicationService communicationService = communicationService;
    private readonly IUmaReclassifyService umaReclassifyService = umaReclassifyService;
    private readonly IBulkSetUnusedService bulkSetUnusedService = bulkSetUnusedService;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'uma-reclassify' route.
    /// </summary>
    /// <param name="req">The HTTP request.</param>
    /// <param name="caseId">The case Id.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: nameof(UmaReclassify), tags: ["Material"], Description = "Represents a function that automatically sets materials to unused.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiRequestBody("application/json", typeof(IReadOnlyCollection<MatchedCommunication>), Description = "Body containing the Request info.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK)]
    [Function("UmaReclassify")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.UmaReclassify)] HttpRequest req, int caseId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} UmaReclassify function processed a request.");

            if (caseId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
            }

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(req);

            // Get the communications
            IReadOnlyCollection<Communication> communications = null;
            communications = await this.GetCommunicationsAsync(caseId, cmsAuthValues).ConfigureAwait(false);

            if (communications == null || communications.Count == 0)
            {
                this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} No communications found for caseId [{caseId}]");
                return new NotFoundObjectResult($"{LoggingConstants.HskUiLogPrefix} No communications found for caseId [{caseId}]");
            }

            // Call ProcessMatchingRequest service
            IReadOnlyCollection<MatchedCommunication> matchedCommunications = await this.umaReclassifyService.ProcessMatchingRequest(caseId, communications).ConfigureAwait(true);

            // Check matchedCommunications is not empty
            if (matchedCommunications == null || matchedCommunications.Count == 0)
            {
                this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} No matched communications found for caseId [{caseId}]");
                return new NotFoundObjectResult($"{LoggingConstants.HskUiLogPrefix} No matched communications found for caseId [{caseId}]");
            }

            // Convert matchedCommunications to bulkSetUnusedRequests
            IReadOnlyCollection<BulkSetUnusedRequest> bulkSetUnusedRequests = matchedCommunications
                .Select(matchedComm => new BulkSetUnusedRequest
                {
                    materialId = matchedComm.materialId,
                    subject = matchedComm.subject,
                })
                .ToList();

            // Call BulkSetUnused service
            BulkSetUnusedResponse result = await this.bulkSetUnusedService.BulkSetUnusedAsync(caseId, cmsAuthValues, bulkSetUnusedRequests).ConfigureAwait(true);

            if (result.Status?.Equals("failed") == true)
            {
                this.logger.LogError(LoggingConstants.UmaReclassifyOperationFailed, LoggingConstants.HskUiLogPrefix, caseId, stopwatch.Elapsed);
                return new UnprocessableEntityObjectResult(result);
            }

            if (result.Status?.Equals("partial_success") == true)
            {
                this.logger.LogWarning(LoggingConstants.UmaReclassifyOperationPartialSuccess, LoggingConstants.HskUiLogPrefix, caseId, stopwatch.Elapsed);
                return new ObjectResult(result) { StatusCode = StatusCodes.Status207MultiStatus };
            }

            this.logger.LogInformation(LoggingConstants.UmaReclassifyOperationCompleted, LoggingConstants.HskUiLogPrefix, caseId, stopwatch.Elapsed);

            var response = new OkObjectResult(result);

            // Set both cache and security headers
            ResponseHeaderHelper.SetNoCacheHeaders(req.HttpContext.Response);
            ResponseHeaderHelper.SetSecurityHeaders(req.HttpContext.Response);

            return response;
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} {ex.Message}");
            return new UnprocessableEntityObjectResult($"{LoggingConstants.HskUiLogPrefix} {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} UmaReclassify function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Asynchronously retrieves communications based on the case ID and authorization values.
    /// </summary>
    /// <param name="caseId">The ID of the case.</param>
    /// <param name="cmsAuthValues">The authorization values.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see cref="Communication"/> objects.</returns>
    private async Task<IReadOnlyCollection<Communication>> GetCommunicationsAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Retrieving communications for caseId [{caseId}]...");
        return await this.communicationService.GetCommunicationsAsync(caseId, cmsAuthValues).ConfigureAwait(false);
    }
}
