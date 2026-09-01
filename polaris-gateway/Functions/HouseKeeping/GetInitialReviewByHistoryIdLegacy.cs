// <copyright file="GetInitialReviewByHistoryIdLegacy.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions.HouseKeeping;

using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Exceptions;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

/// <summary>
/// Represents a function that return Case history initial review data by case id and history event id.
/// intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetInitialReviewByHistoryIdLegacy"/> class.
/// </remarks>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="communicationService">The service used to get call PCD Review service.</param>
public class GetInitialReviewByHistoryIdLegacy(
    ILogger<GetInitialReviewByHistoryIdLegacy> logger,
    ICommunicationService communicationService): BaseFunction(logger)
{
    private readonly ILogger<GetInitialReviewByHistoryIdLegacy> logger = logger;
    private readonly ICommunicationService communicationService = communicationService;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'case/{caseId}/history/initial-review' route.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="caseId">The case id to get PCD Review.</param>
    /// <param name="historyId">History event id.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: nameof(GetInitialReviewByHistoryIdLegacy), tags: ["CaseHistory"], Description = "Returns case history initial review with Case Id and history event id.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to get initial review.", Required = true)]
    [OpenApiParameter("historyId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the history event to get initial review.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Cps.MasterDataService.Infrastructure.ApiClient.PreChargeDecisionAnalysisOutcome), Description = "Return initial review.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError)]
    [Function(nameof(GetInitialReviewByHistoryIdLegacy))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.InitialReviewByHistoryIdLegacy)] HttpRequest request, int caseId, int historyId, CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} GetInitialReviewByHistoryIdLegacy function processed a request.");

            if (caseId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
            }

            if (historyId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid history Id. It should be an integer.");
            }

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(request);

            var result = await this.communicationService.GetInitialReviewByHistoryIdAsync(caseId, historyId, cmsAuthValues, cancellationToken).ConfigureAwait(true);

            if (result == null)
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] GetInitialReviewByHistoryIdLegacy function failed in [{stopwatch.Elapsed}]");
                return new UnprocessableEntityObjectResult(result);
            }

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] GetInitialReviewByHistoryIdLegacy function completed in [{stopwatch.Elapsed}]");

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
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} GetInitialReviewByHistoryIdLegacy function encountered unsupported content type.");
            return new UnprocessableEntityObjectResult($"GetInitialReviewByHistoryIdLegacy error: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} GetInitialReviewByHistoryIdLegacy function encountered UnauthorizedAccess Exception.");
            return new UnauthorizedObjectResult($"GetInitialReviewByHistoryIdLegacy error: {ex.Message}");
        }
        catch (NotFoundException ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetPreChargeDecisionByHistoryId function return not found error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status404NotFound);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetInitialReviewByHistoryIdLegacy function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
