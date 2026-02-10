// <copyright file="GetCaseWitnessStatements.cs" company="TheCrownProsecutionService">
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
/// A function that retrieves statements for a witness.
/// </summary>
/// <param name="logger">The logger instance used for logging.</param>
/// <param name="witnessService">The service used to process request and return result.</param>
/// <param name="cookieService">Handles cookie related operations.</param>
public class GetCaseWitnessStatements(
    ILogger<GetCaseWitnessStatements> logger,
    IWitnessService witnessService) : BaseFunction(logger)
{
    private readonly ILogger<GetCaseWitnessStatements> logger = logger;
    private readonly IWitnessService witnessService = witnessService;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'api/case-witnesses' route.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="caseId">The case Id.</param>
    /// <param name="witnessId">The Id of the witness to get statements for.</param>
    /// <returns>An <see cref="IActionResult"/> The response of the function.</returns>
    [OpenApiOperation(operationId: nameof(GetCaseWitnessStatements), tags: ["Statement"], Description = "Represents a function that retrieves statements for a witness.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(WitnessStatementsResponse), Description = "Return success response with body.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized)]
    [Function(nameof(GetCaseWitnessStatements))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseWitnessStatements)] HttpRequest request, int caseId, int witnessId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} {nameof(GetCaseWitnessStatements)} function processed a request.");

            if (witnessId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid witness_id format. It should be an integer.");
            }

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(request);

            WitnessStatementsResponse? result = await this.witnessService.GetWitnessStatementsAsync(witnessId, cmsAuthValues).ConfigureAwait(false);

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] witnessId [{witnessId}] {nameof(GetCaseWitnessStatements)} function completed in [{stopwatch.Elapsed}]");

            var response = new OkObjectResult(result);

            // Set both cache and security headers
            ResponseHeaderHelper.SetNoCacheHeaders(request.HttpContext.Response);
            ResponseHeaderHelper.SetSecurityHeaders(request.HttpContext.Response);

            return response;
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} {nameof(GetCaseWitnessStatements)} function encountered an invalid operation error: {ex.Message}");
            return new UnprocessableEntityObjectResult($"{ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} {nameof(GetCaseWitnessStatements)} function encountered an unauthorized access error: {ex.Message}");
            return new UnauthorizedObjectResult($"{nameof(GetCaseWitnessStatements)} error: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} {nameof(GetCaseWitnessStatements)} function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
