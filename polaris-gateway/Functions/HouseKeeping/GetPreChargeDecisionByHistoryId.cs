// <copyright file="GetPreChargeDecisionByHistoryId.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Functions.Functions.CaseHistory;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces;
using System.Diagnostics;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Microsoft.OpenApi.Models;
using System.Net;
using System;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Cps.Fct.Hk.Common.DDEI.Provider.Contracts;
using Cps.Fct.Hk.Common.DDEI.Provider.Models.Response.PcdReview;
using Cps.Fct.Hk.Common.DDEI.Provider.Models.Request;
using Cps.Fct.Hk.Common.DDEI.Provider.Models.Response.CaseHistory;
using Cps.Fct.Hk.Common.Contracts.Exceptions;

/// <summary>
/// Represents a function that return case pre charge decision by case id and history event id,
/// intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetPreChargeDecisionByHistoryId"/> class.
/// </remarks>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="caseHistoryEventProvider">The service used to get call case history service.</param>
/// <param name="cookieService">The service used to handle cookie-related operations.</param>
public class GetPreChargeDecisionByHistoryId(
    ILogger<GetPreChargeDecisionByHistoryId> logger,
    ICaseHistoryEventProvider caseHistoryEventProvider,
    ICookieService cookieService)
{
    private readonly ILogger<GetPreChargeDecisionByHistoryId> logger = logger;
    private readonly ICaseHistoryEventProvider caseHistoryEventProvider = caseHistoryEventProvider;
    private readonly ICookieService cookieService = cookieService;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'case/{caseId}/pcd-review' route.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="caseId">The case id to get PCD Review.</param>
    /// <param name="historyId">History event id.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: nameof(GetPreChargeDecisionByHistoryId), tags: ["CaseHistory"], Description = "Returns PCD with Case Id and history event id and history event id.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to get PCD.", Required = true)]
    [OpenApiParameter("historyId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to get PCD.", Required = true)]
    [OpenApiRequestBody("application/json", typeof(PcdReviewResponse), Description = "Body containing the PCD info.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PreChargeDecisionOutcome), Description = "Return PCD review.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError)]
    [Function(nameof(GetPreChargeDecisionByHistoryId))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cases/{caseId}/history/{historyId}/pre-charge-decision")] HttpRequest request, int caseId, int historyId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} GetPreChargeDecisionByHistoryId function processed a request.");

            (bool isValid, string? errorMessage, int? caseIdFromCookie) = this.cookieService.ValidateCookies(request);
            if (!isValid)
            {
                return new BadRequestObjectResult(errorMessage);
            }

            if (caseIdFromCookie != caseId)
            {
                return new BadRequestObjectResult($"Invalid case id: {caseId}");
            }

            CmsAuthValues cmsAuthValues = this.GetCmsAuthValues(request);

            string requestBody = await new StreamReader(request.Body).ReadToEndAsync().ConfigureAwait(false);
            var getPreChargeDecisionByHistoryIdRequest = new GetCaseHistoryByHistoryIdRequest(caseId, historyId, Guid.NewGuid());

            var cookie = new Common.DDEI.Client.Model.CmsAuthValues(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
            Common.DDEI.Provider.Models.Response.CaseHistory.PreChargeDecisionOutcome? result = await this.caseHistoryEventProvider.GetPreChargeDecisionByIdAsync(getPreChargeDecisionByHistoryIdRequest, cookie).ConfigureAwait(true);

            if (result == null)
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] GetPreChargeDecisionByHistoryId function failed in [{stopwatch.Elapsed}]");
                return new UnprocessableEntityObjectResult(result);
            }

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] GetPreChargeDecisionByHistoryId function completed in [{stopwatch.Elapsed}]");

            var response = new OkObjectResult(result);

            return response;
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogError($"{ex.Message}");
            return new UnprocessableEntityObjectResult($"{ex.Message}");
        }
        catch (NotSupportedException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} GetPreChargeDecisionByHistoryId function encountered unsupported content type.");
            return new UnprocessableEntityObjectResult($"GetPreChargeDecisionByHistoryId error: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} GetPreChargeDecisionByHistoryId function encountered UnauthorizedAccess Exception.");
            return new UnauthorizedObjectResult($"GetPreChargeDecisionByHistoryId error: {ex.Message}");
        }
        catch (NotFoundException ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetPreChargeDecisionByHistoryId function return not found error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetPreChargeDecisionByHistoryId function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Retrieves authorization values from the incoming HTTP request.
    /// </summary>
    /// <param name="req">The HTTP request.</param>
    /// <returns>A <see cref="CmsAuthValues"/> object containing authorization details.</returns>
    private CmsAuthValues GetCmsAuthValues(HttpRequest req)
    {
        return new CmsAuthValues(
            this.cookieService.GetCmsCookies(req) ?? string.Empty,
            this.cookieService.GetCmsToken(req) ?? string.Empty,
            Guid.NewGuid());
    }
}
