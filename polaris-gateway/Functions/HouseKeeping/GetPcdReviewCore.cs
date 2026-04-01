// <copyright file="GetPcdReviewCore.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions.HouseKeeping;

using Common.Configuration;
using Common.Constants;
using Common.Dto.Response.HouseKeeping;
using Common.Exceptions;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Represents a function that return PCD Review details by case id and PCD analysis id,
/// intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetPcdReviewCore"/> class.
/// </remarks>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="communicationService">The service used to get call PCD Review service.</param>
public class GetPcdReviewCore(ILogger<GetPcdReviewCore> logger, ICommunicationService communicationService) : BaseFunction(logger)
{
    private readonly ILogger<GetPcdReviewCore> logger = logger;
    private readonly ICommunicationService communicationService = communicationService;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'case/{caseId}/pcd-review' route.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="caseId">The case id to get PCD Review.</param>
    /// <param name="cancellationToken">The cancellationToken.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: nameof(GetPcdReviewCore), tags: ["PCDReview"], Description = "Returns PCD Review with Case Id.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to get PCD review.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IReadOnlyCollection<PcdReviewCoreResponseDto>), Description = "Return PCD review.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError)]
    [Function(nameof(GetPcdReviewCore))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.PcdReviewCore)] HttpRequest request, string caseUrn, int caseId, CancellationToken cancellationToken)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation("{HskUiLogPrefix} GetPcdReviewCore function processed a request.", LoggingConstants.HskUiLogPrefix);

            if (caseId < 1)
            {
                return new BadRequestObjectResult(
                                      string.Format("{0} Invalid case Id. It should be an integer.", LoggingConstants.HskUiLogPrefix)
                                  );
            }

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(request);

            var result = await this.communicationService.GetPcdReviewCoreAsync(caseId, cmsAuthValues, cancellationToken);

            if (result == null)
            {
                this.logger.LogError("{HskUiLogPrefix} caseId {CaseId} GetPcdReviewCore function failed in {Elapsed}", LoggingConstants.HskUiLogPrefix, caseId, stopwatch.Elapsed);
                return new UnprocessableEntityObjectResult(result);
            }

            this.logger.LogInformation("{HskUiLogPrefix} Milestone: caseId {CaseId} GetPcdReviewCore function completed in {Elapsed}", LoggingConstants.HskUiLogPrefix, caseId, stopwatch.Elapsed);

            var response = new OkObjectResult(result.ToList());

            return response;
        }
        catch (BadRequestException ex)
        {
            this.logger.LogError(ex, "{Message}", ex.Message);
            return new BadRequestObjectResult(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogError(ex, "{Message}", ex.Message);
            return new UnprocessableEntityObjectResult(ex.Message);
        }
        catch (NotSupportedException ex)
        {
            this.logger.LogError(ex, "{HskUiLogPrefix} GetPcdReviewCore function encountered unsupported content type.", LoggingConstants.HskUiLogPrefix);
            return new UnprocessableEntityObjectResult($"GetPcdReviewCore error: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            this.logger.LogError(ex, "{HskUiLogPrefix} GetPcdReviewCore function encountered UnauthorizedAccess Exception.", LoggingConstants.HskUiLogPrefix);
            return new UnauthorizedObjectResult($"GetPcdReviewCore error: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "{HskUiLogPrefix} GetPcdReviewCore function encountered an error: {Message}", LoggingConstants.HskUiLogPrefix, ex.Message);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
