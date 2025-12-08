// <copyright file="CompleteReclassification.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Functions.Functions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using System.Diagnostics;
using Newtonsoft.Json;
using Cps.Fct.Hk.Ui.Services.Validators;
using PolarisGateway.Functions;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Common.Constants;
using Common.Dto.Request;
using Aspose.Pdf.Operators;
using System.IO;
using System;
using Common.Configuration;

/// <summary>
/// A function that orchestrates all reclassification operations (Add witness, add action plan, reclassify and rename)
/// of a case material (MG Forms, Other, Statement and Exhibit).
/// </summary>
/// <param name="logger">The logger instance used for logging.</param>
/// <param name="reclassificationOrchestrationService">The service used to process complete reclassification request and return operation results.</param>
/// <param name="cookieService">Handles cookie related operations.</param>
/// <param name="completeReclassificationRequestValidator">Validates the request.</param>
public class CompleteReclassification(
    ILogger<CompleteReclassification> logger,
    IMaterialReclassificationOrchestrationService reclassificationOrchestrationService,
    CompleteReclassificationRequestValidator completeReclassificationRequestValidator) : BaseFunction(logger)
{
    private readonly ILogger<CompleteReclassification> logger = logger;
    private readonly IMaterialReclassificationOrchestrationService reclassificationOrchestrationService = reclassificationOrchestrationService;
    private readonly CompleteReclassificationRequestValidator requestValidator = completeReclassificationRequestValidator;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'material/reclassify-complete' route.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="caseId">The case Id.</param>
    /// <param name="materialId">The material id belonging to the case material to reclassify.</param>
    /// <returns>An <see cref="IActionResult"/> The response of the function.</returns>
    [OpenApiOperation(operationId: "CompleteReclassification", tags: ["Material"], Description = "Represents a function that orchestrates all reclassification operations of a case material (MG Forms, Other, Statement and Exhibit).")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(int), Description = "The material id request parameter.", Required = true)]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The case id request parameter.", Required = true)]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CompleteReclassificationRequest), Required = true, Description = "The complete reclassification request body.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CompleteReclassificationResponse), Description = "Return results of all reclassification operations in body.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.MultiStatus)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized)]
    [Function("CompleteReclassification")]
    public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.CompleteReclassification)]
    HttpRequest request, int caseId, int materialId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} CompleteReclassification function processed a request.");

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

            CompleteReclassificationRequest? completeReclassificationRequest = JsonConvert.DeserializeObject<CompleteReclassificationRequest>(requestBody);

            if (completeReclassificationRequest == null)
            {
                return new BadRequestObjectResult("completeReclassificationRequest is null or empty");
            }

            FluentValidation.Results.ValidationResult validationResult = this.requestValidator.Validate(completeReclassificationRequest);
            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(validationResult.Errors.ToArray());
            }

            CompleteReclassificationResponse? result = await this.reclassificationOrchestrationService.CompleteReclassificationAsync(
                caseId,
                materialId,
                cmsAuthValues,
                completeReclassificationRequest).ConfigureAwait(false);

            // All operations succeeded.
            if (result?.overallSuccess == true)
            {
                this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] CompleteReclassification function completed in [{stopwatch.Elapsed}]");
                return new OkObjectResult(result);
            }

            // Some operations succeeded and some failed.
            if (result?.overallSuccess == false && result?.status == "PartialSuccess")
            {
                var multiStatusObjectResult = new ObjectResult(result)
                {
                    StatusCode = 207,
                };
                this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] CompleteReclassification function completed in [{stopwatch.Elapsed}]");
                return multiStatusObjectResult;
            }

            // All operations failed.
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} CompleteReclassification function encountered an invalid operation error: {result?.errors}");
            return new UnprocessableEntityObjectResult(result ?? null);
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} CompleteReclassification function encountered an invalid operation error: {ex.Message}");
            return new UnprocessableEntityObjectResult($"{ex.Message}");
        }
        catch (NotSupportedException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} CompleteReclassification function encountered an unsupported content type error: {ex.Message}");
            return new UnprocessableEntityObjectResult($"ReclassifyCaseMaterial error: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} CompleteReclassification function encountered an unauthorized access error: {ex.Message}");
            return new UnauthorizedObjectResult($"CompleteReclassification error: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} CompleteReclassification function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
