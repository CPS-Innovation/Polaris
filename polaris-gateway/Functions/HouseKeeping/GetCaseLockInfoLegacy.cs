// <copyright file="GetCaseLockInfoLegacy.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions.HouseKeeping;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces;
using System.Diagnostics;
using Cps.Fct.Hk.Ui.Interfaces.Exceptions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using PolarisGateway.Functions;
using Common.Dto.Response.HouseKeeping;
using Common.Constants;
using System;
using PolarisGateway.Helpers;
using Common.Configuration;

/// <summary>
/// Represents a function that retrieves the case lock info,
/// intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetCaseLockInfoLegacy"/> class.
/// </remarks>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="caseLockService">The service used to process the request and generate the result.</param>
/// <param name="cookieService">The service used to handle cookie-related operations.</param>
public class GetCaseLockInfoLegacy(ILogger<GetCaseLockInfoLegacy> logger, ICaseLockService caseLockService) : BaseFunction(logger)
{
    private readonly ILogger<GetCaseLockInfoLegacy> logger = logger;
    private readonly ICaseLockService caseLockService = caseLockService;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'case-lock-info' route.
    /// </summary>
    /// <param name="req">The HTTP request.</param>
    /// <param name="caseId">The case Id.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: "GetCaseLockInfoLegacy", tags: ["Case"], Description = "Represents a function that retrieves the case lock information.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiRequestBody("application/json", typeof(CaseLockedStatusResult), Description = "Return case lock response.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK)]
    [Function(nameof(GetCaseLockInfoLegacy))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseLockInfoLegacy)] HttpRequest req, int caseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} GetCaseLockInfoLegacy function processed a request.");

            if (caseId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
            }

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(req);

            CaseLockedStatusResult caseLockSummary;
            try
            {
                caseLockSummary = await this.caseLockService.CheckCaseLockAsync(caseId, cmsAuthValues, cancellationToken).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetCaseLockInfoLegacy function encountered an error fetching case lock information for caseId [{caseId}]: {ex.Message}");
                throw new UnprocessableEntityException($"GetCaseLockInfoLegacy function encountered an error fetching case lock information for caseId [{caseId}] - Error: [{ex.Message}]");
            }

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] GetCaseLockInfoLegacy function completed in [{stopwatch.Elapsed}]");
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}]");

            var response = new OkObjectResult(caseLockSummary);

            ResponseHeaderHelper.SetNoCacheHeaders(req.HttpContext.Response);
            ResponseHeaderHelper.SetSecurityHeaders(req.HttpContext.Response);

            return response;
        }
        catch (UnprocessableEntityException ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetCaseLockInfoLegacy function encountered an unprocessable entity error: {ex.Message}");
            return new ObjectResult(ex.Message)
            {
                StatusCode = StatusCodes.Status422UnprocessableEntity,
            };
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} {ex.Message}");
            return new UnprocessableEntityObjectResult($"{ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetCaseLockInfoLegacy function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
