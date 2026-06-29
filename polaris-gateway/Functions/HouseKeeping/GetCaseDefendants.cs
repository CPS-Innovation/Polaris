// <copyright file="GetCaseDefendants.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions.HouseKeeping;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces;
using System.Diagnostics;
using Cps.Fct.Hk.Ui.Interfaces.Exceptions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Common.Dto.Response.HouseKeeping;
using Common.Constants;
using Common.Dto.Request;
using Azure.Core;
using PolarisGateway.Helpers;
using System;
using Common.Configuration;

/// <summary>
/// Represents a function that retrieves the case defendants,
/// intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetCaseDefendants"/> class.
/// </remarks>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="caseDefendantsService">The service used to process the request and generate the result.</param>
public class GetCaseDefendants(
            ILogger<GetCaseDefendants> logger, 
            ICaseDefendantsService caseDefendantsService) : BaseFunction(logger)
{
    private readonly ILogger<GetCaseDefendants> logger = logger;
    private readonly ICaseDefendantsService caseDefendantsService = caseDefendantsService;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'case-defendants' route.
    /// </summary>
    /// <param name="req">The HTTP request.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: "GetCaseDefendants", tags: ["Case"], Description = "Represents a function that retrieves the case defendants for display purposes.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiRequestBody("application/json", typeof(Defendant), Description = "Return case defendants response.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK)]
    [Function("GetCaseDefendants")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseDefendants)] HttpRequest request, int caseId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} GetCaseDefendants function processed a request.");

            if (caseId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
            }

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(request);

            DefendantsResponse? caseDefendants;
            try
            {
                caseDefendants = await this.caseDefendantsService.GetCaseDefendantsAsync(caseId, cmsAuthValues).ConfigureAwait(true);
                this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] GetCaseDefendants function completed in [{stopwatch.Elapsed}]");

                var response = new OkObjectResult(caseDefendants);

                // Set both cache and security headers
                ResponseHeaderHelper.SetNoCacheHeaders(request.HttpContext.Response);
                ResponseHeaderHelper.SetSecurityHeaders(request.HttpContext.Response);

                return response;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetCaseDefendants function encountered an error fetching case defendants for caseId [{caseId}]: {ex.Message}");
                throw new UnprocessableEntityException($"GetCaseDefendants function encountered an error fetching case defendants for caseId [{caseId}]");
            }
        }
        catch (UnprocessableEntityException ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetCaseDefendants function encountered an unprocessable entity error: {ex.Message}");
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
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetCaseDefendants function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
