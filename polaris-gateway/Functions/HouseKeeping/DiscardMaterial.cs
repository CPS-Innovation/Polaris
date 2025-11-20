// <copyright file="DiscardMaterial.cs" company="TheCrownProsecutionService">
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
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Common.Constants;
using System.IO;
using System;
using Common.Configuration;

/// <summary>
/// Represents a function that discards a material,
/// intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DiscardMaterial"/> class.
/// </remarks>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="communicationService">The service used to process the request and generate the result.</param>
public class DiscardMaterial(
    ILogger<DiscardMaterial> logger,
    ICommunicationService communicationService) : BaseFunction(logger)
{
    private readonly ILogger<DiscardMaterial> logger = logger;
    private readonly ICommunicationService communicationService = communicationService;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'material/discard' route.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: "DiscardMaterial", tags: ["Material"], Description = "Represents a function that discards the material.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(DiscardMaterialRequest), Required = true, Description = "The discard material request body.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DiscardMaterialResponse), Description = "Return success response with body.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized)]
    [Function("DiscardMaterial")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = RestApi.DiscardMaterial)] HttpRequest request, int caseId, int materialId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} DiscardMaterial function processed a request.");
            if (caseId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
            }

            if (materialId < 0)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid material_id format. It should be an integer.");
            }

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(request);

            string requestBody = await new StreamReader(request.Body).ReadToEndAsync().ConfigureAwait(false);

            DiscardMaterialRequest? discardMaterialRequest = JsonConvert.DeserializeObject<DiscardMaterialRequest>(requestBody);

            if (string.IsNullOrWhiteSpace(discardMaterialRequest?.discardReason))
            {
                return new BadRequestObjectResult(nameof(discardMaterialRequest.discardReason));
            }

            if (string.IsNullOrWhiteSpace(discardMaterialRequest?.discardReasonDescription))
            {
                return new BadRequestObjectResult(nameof(discardMaterialRequest.discardReasonDescription));
            }

            DiscardMaterialResponse? result = await this.communicationService.DiscardMaterialAsync(
                caseId,
                discardMaterialRequest.materialId,
                discardMaterialRequest.discardReason,
                discardMaterialRequest.discardReasonDescription,
                cmsAuthValues).ConfigureAwait(true);

            if (result?.DiscardMaterialData?.Id == null)
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] DiscardMaterial function failed in [{stopwatch.Elapsed}]");
                return new UnprocessableEntityObjectResult(result);
            }

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] DiscardMaterial function completed in [{stopwatch.Elapsed}]");

            var response = new OkObjectResult(result);

            return response;
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} DiscardMaterial function encountered an invalid operation error: {ex.Message}");
            return new UnprocessableEntityObjectResult($"{ex.Message}");
        }
        catch (NotSupportedException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} DiscardMaterial function encountered an unsupported content type error: {ex.Message}");
            return new UnprocessableEntityObjectResult($"Discard error: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} DiscardMaterial function encountered an unauthorized access error: {ex.Message}");
            return new UnauthorizedObjectResult($"Discard error: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} DiscardMaterial function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
