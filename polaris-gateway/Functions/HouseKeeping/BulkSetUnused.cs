// <copyright file="BulkSetUnused.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions.HouseKeeping;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;
using Common.Configuration;
using Common.Constants;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
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
/// Represents a function that bulk sets materials to unused,
/// intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="BulkSetUnused"/> class.
/// </remarks>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="bulkSetUnusedService">The service used to process the request and generate the result.</param>
/// <param name="cookieService">The service used to handle cookie-related operations.</param>
public class BulkSetUnused(ILogger<BulkSetUnused> logger, IBulkSetUnusedService bulkSetUnusedService) : BaseFunction(logger)
{
    private readonly ILogger<BulkSetUnused> logger = logger;
    private readonly IBulkSetUnusedService bulkSetUnusedService = bulkSetUnusedService;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'case-materials/bulk-set-unused' route.
    /// </summary>
    /// <param name="req">The HTTP request.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: "BulkSetUnused", tags: ["Material"], Description = "Represents a function that bulk sets materials to unused.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiRequestBody("application/json", typeof(BulkSetUnusedResponse), Description = "Return success response.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK)]
    [Function("BulkSetUnused")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.BulkSetUnused)] HttpRequest req, int caseId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} BulkSetUnused function processed a request.");

            if (caseId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
            }

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(req);

            // Deserialize the request body into IReadOnlyCollection<BulkSetUnusedRequest>
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(true);

            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid or empty request body.");
            }

            IReadOnlyCollection<BulkSetUnusedRequest>? bulkSetUnusedRequests = JsonSerializer.Deserialize<IReadOnlyCollection<BulkSetUnusedRequest>>(requestBody);

            if (bulkSetUnusedRequests == null || bulkSetUnusedRequests.Count == 0)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid or empty request body.");
            }

            BulkSetUnusedResponse result = await this.bulkSetUnusedService.BulkSetUnusedAsync(caseId, cmsAuthValues, bulkSetUnusedRequests).ConfigureAwait(true);

            if (result.Status?.Equals("failed") == true)
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] BulkSetUnused function failed in [{stopwatch.Elapsed}]");
                return new UnprocessableEntityObjectResult(result);
            }

            if (result.Status?.Equals("partial_success") == true)
            {
                this.logger.LogWarning($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] BulkSetUnused function completed partially successfully in [{stopwatch.Elapsed}]");
                return new ObjectResult(result) { StatusCode = StatusCodes.Status207MultiStatus };
            }

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] BulkSetUnused function completed sucessfully in [{stopwatch.Elapsed}]");

            var response = new OkObjectResult(result);

            // Set both cache and security headers
            ResponseHeaderHelper.SetNoCacheHeaders(req.HttpContext.Response);
            ResponseHeaderHelper.SetSecurityHeaders(req.HttpContext.Response);

            return response;
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} BulkSetUnused function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
