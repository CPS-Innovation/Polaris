// <copyright file="GetInitialReview.cs" company="TheCrownProsecutionService">
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
using Microsoft.OpenApi.Models;
using System.Net;
using System;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Cps.Fct.Hk.Common.DDEI.Provider.Contracts;
using Cps.Fct.Hk.Common.DDEI.Provider.Models.Request;
using Cps.Fct.Hk.Common.DDEI.Provider.Models.Response.CaseHistory;
using Common.Constants;
using System.IO;
using Cps.Fct.Hk.Common.DDEI.Client.Model;
using Common.Configuration;

/// <summary>
/// Represents a function that return Case history initial review data by case id.
/// intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetInitialReview"/> class.
/// </remarks>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="caseHistoryEventProvider">The service used to get call PCD Review service.</param>
/// <param name="cookieService">The service used to handle cookie-related operations.</param>
public class GetInitialReview(
    ILogger<GetInitialReview> logger,
    ICaseHistoryEventProvider caseHistoryEventProvider) : BaseFunction(logger)
{
    private readonly ILogger<GetInitialReview> logger = logger;
    private readonly ICaseHistoryEventProvider caseHistoryEventProvider = caseHistoryEventProvider;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'case/{caseId}/history/initial-review' route.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="caseId">The case id to get PCD Review.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: nameof(GetInitialReview), tags: ["CaseHistory"], Description = "Returns case history initial review with Case Id.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to get initial review.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PreChargeDecisionAnalysisOutcome), Description = "Return initial review.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError)]
    [Function(nameof(GetInitialReview))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.InitialReviewByHistoryId)] HttpRequest request, int caseId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} GetInitialReview function processed a request.");

            if (caseId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
            }

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(request);

            string requestBody = await new StreamReader(request.Body).ReadToEndAsync().ConfigureAwait(false);
            var getcaseHistoryRequest = new GetCaseHistoryEventRequest(caseId, Guid.NewGuid());

            var cookie = new CmsAuthValues(cmsAuthValues.CmsCookies, cmsAuthValues.CmsModernToken);
            PreChargeDecisionAnalysisOutcome result = await this.caseHistoryEventProvider.GetInitialReviewCaseHistoryEventDetailsAsync(getcaseHistoryRequest, cookie).ConfigureAwait(true);

            if (result == null)
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] GetInitialReview function failed in [{stopwatch.Elapsed}]");
                return new UnprocessableEntityObjectResult(result);
            }

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] GetInitialReview function completed in [{stopwatch.Elapsed}]");

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
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} GetInitialReview function encountered unsupported content type.");
            return new UnprocessableEntityObjectResult($"GetInitialReview error: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} GetInitialReview function encountered UnauthorizedAccess Exception.");
            return new UnauthorizedObjectResult($"GetInitialReview error: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetInitialReview function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
