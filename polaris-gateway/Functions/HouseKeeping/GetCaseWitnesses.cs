// <copyright file="GetCaseWitnesses.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions.HouseKeeping;

using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Functions;
using PolarisGateway.Helpers;

/// <summary>
/// A function that retrieves witnesses for a case.
/// </summary>
/// <param name="logger">The logger instance used for logging.</param>
/// <param name="witnessService">The service used to process request and return result.</param>
/// <param name="cookieService">Handles cookie related operations.</param>
public class GetCaseWitnesses(ILogger<GetCaseWitnesses> logger,
    IWitnessService witnessService) : BaseFunction(logger)
{
    private readonly ILogger<GetCaseWitnesses> logger = logger;
    private readonly IWitnessService witnessService = witnessService;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'api/case-witnesses' route.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="caseId">The case Id.</param>
    /// <returns>An <see cref="IActionResult"/> The response of the function.</returns>
    [OpenApiOperation(operationId: nameof(GetCaseWitnesses), tags: ["Witness"], Description = "Represents a function that retrieves witnesses for a case.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(WitnessesResponse), Description = "Return success response with body.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized)]
    [Function(nameof(GetCaseWitnesses))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseWitnessesHk)] HttpRequest request, int caseId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} {nameof(GetCaseWitnesses)} function processed a request.");

            if (caseId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
            }

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = BuildCmsAuthValues(request);

            WitnessesResponse result = await witnessService.GetCaseWitnessesAsync(caseId, cmsAuthValues).ConfigureAwait(false);

            logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] {nameof(GetCaseWitnesses)} function completed in [{stopwatch.Elapsed}]");

            var response = new OkObjectResult(result);

            // Set both cache and security headers
            ResponseHeaderHelper.SetNoCacheHeaders(request.HttpContext.Response);
            ResponseHeaderHelper.SetSecurityHeaders(request.HttpContext.Response);

            return response;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError($"{LoggingConstants.HskUiLogPrefix} {nameof(GetCaseWitnesses)} function encountered an invalid operation error: {ex.Message}");
            return new UnprocessableEntityObjectResult($"{ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} {nameof(GetCaseWitnesses)} function encountered an unauthorized access error: {ex.Message}");
            return new UnauthorizedObjectResult($"{nameof(GetCaseWitnesses)} error: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError($"{LoggingConstants.HskUiLogPrefix} {nameof(GetCaseWitnesses)} function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
