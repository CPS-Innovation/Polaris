// <copyright file="GetPCDRequestByPcdId.cs" company="TheCrownProsecutionService">
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
using Common.Dto.Response.HouseKeeping.Pcd;
using Common.Constants;
using Common.Dto.Request;
using System.IO;
using Common.Configuration;

/// <summary>
/// Represents a function that return PCD Request information by case id and PCD id,
/// intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetPcdRequestByPcdId"/> class.
/// </remarks>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="communicationService">The service used to get call pcd request service.</param>
public class GetPcdRequestByPcdId(
    ILogger<GetPcdRequestByPcdId> logger,
    ICommunicationService communicationService) : BaseFunction(logger)
{
    private readonly ILogger<GetPcdRequestByPcdId> logger = logger;
    private readonly ICommunicationService communicationService = communicationService;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'case/{caseId}/pcd-request/{pcdId}' route.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="caseId">The case id to get PCD requests.</param>
    /// <param name="pcdId">The PCD id to get single PCD request overview.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: nameof(GetPcdRequestByPcdId), tags: ["Pre-Charge Decision"], Description = "Returns PCD Request by PCD ID along with Case Id.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to get PCD Request.", Required = true)]
    [OpenApiParameter("pcdId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the PCD Request to get PCD Request.", Required = true)]
    [OpenApiRequestBody("application/json", typeof(PcdRequestDto), Description = "Body containing the PCD Request info.", Example = typeof(GetPcdRequestByPcdId))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PcdRequestDto), Description = "Return PCD Request.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError)]
    [Function(nameof(GetPcdRequestByPcdId))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.PcdRequest)] HttpRequest request, int caseId, int pcdId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} GetPCDRequestByPcdId function processed a request.");

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(request);

            string requestBody = await new StreamReader(request.Body).ReadToEndAsync().ConfigureAwait(false);

            PcdRequestDto? result = await this.communicationService.GetPcdRequestByPcdIdAsync(caseId, pcdId, cmsAuthValues).ConfigureAwait(true);

            if (result == null)
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] GetPCDRequestByPcdId function failed in [{stopwatch.Elapsed}]");
                return new UnprocessableEntityObjectResult(result);
            }

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] GetPCDRequestByPcdId function completed in [{stopwatch.Elapsed}]");

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
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} GetPCDRequestByPcdId function encountered unsupported content type.");
            return new UnprocessableEntityObjectResult($"GetPCDRequestByPcdId error: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} GetPCDRequestByPcdId function encountered UnauthorizedAccess Exception.");
            return new UnauthorizedObjectResult($"GetPCDRequestByPcdId error: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetPCDRequestByPcdId function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
