// <copyright file="GetCaseInfo.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>


using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces;
using System.Diagnostics;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Cps.Fct.Hk.Ui.Functions.Utils;
using Cps.Fct.Hk.Ui.Interfaces.Exceptions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using System;
using Common.Configuration;
using PolarisGateway.Functions;

namespace Cps.Fct.Hk.Ui.Functions.Functions;
/// <summary>
/// Represents a function that retrieves the case information for display purposes,
/// intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetCaseInfo"/> class.
/// </remarks>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="caseInfoService">The service used to process the request and generate the result.</param>
/// <param name="cookieService">The service used to handle cookie-related operations.</param>
public class GetCaseInfo(ILogger<GetCaseInfo> logger, ICaseInfoService caseInfoService) : BaseFunction(logger)
{
    private readonly ILogger<GetCaseInfo> logger = logger;
    private readonly ICaseInfoService caseInfoService = caseInfoService;
   
    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'case-info' route.
    /// </summary>
    /// <param name="req">The HTTP request.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: "GetCaseInfo", tags: ["Case"], Description = "Represents a function that retrieves the case information for display purposes.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiRequestBody("application/json", typeof(CaseSummary), Description = "Return case summary response.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK)]
    [Function("GetCaseInfo")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseInfo)] HttpRequest req, string caseUrn, int caseId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} GetCaseInfo function processed a request.");

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = BuildCmsAuthValues(req);

            CaseSummary caseSummary;
            try
            {
                caseSummary = await this.caseInfoService.GetCaseInfoAsync(caseId, cmsAuthValues).ConfigureAwait(true);
                this.logger.LogInformation(LoggingConstants.UnitNameExtractionSuccess, LoggingConstants.HskUiLogPrefix, caseSummary.UnitName, caseId);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetCaseInfo function encountered an error fetching case information for caseId [{caseId}]: {ex.Message}");
                throw new UnprocessableEntityException($"GetCaseInfo function encountered an error fetching case information for caseId [{caseId}]");
            }

            var response = new OkObjectResult(caseSummary);

            // Set both cache and security headers
            ResponseHeaderHelper.SetNoCacheHeaders(req.HttpContext.Response);
            ResponseHeaderHelper.SetSecurityHeaders(req.HttpContext.Response);

            return response;
        }
        catch (UnprocessableEntityException ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetCaseInfo function encountered an unprocessable entity error: {ex.Message}");
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
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetCaseInfo function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
