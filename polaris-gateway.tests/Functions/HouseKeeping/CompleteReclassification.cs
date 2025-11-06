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
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using System.Diagnostics;
using Newtonsoft.Json;
using Cps.Fct.Hk.Ui.Services.Validators;

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
    ICookieService cookieService,
    CompleteReclassificationRequestValidator completeReclassificationRequestValidator)
{
    private readonly ILogger<CompleteReclassification> logger = logger;
    private readonly IMaterialReclassificationOrchestrationService reclassificationOrchestrationService = reclassificationOrchestrationService;
    private readonly ICookieService cookieService = cookieService;
    private readonly CompleteReclassificationRequestValidator requestValidator = completeReclassificationRequestValidator;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'material/reclassify-complete' route.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="material_id">The material id belonging to the case material to reclassify.</param>
    /// <returns>An <see cref="IActionResult"/> The response of the function.</returns>
    [OpenApiOperation(operationId: "CompleteReclassification", tags: ["Material"], Description = "Represents a function that orchestrates all reclassification operations of a case material (MG Forms, Other, Statement and Exhibit).")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiParameter("material_id", In = ParameterLocation.Path, Type = typeof(int), Description = "The material id request parameter.", Required = true)]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(CompleteReclassificationRequest), Required = true, Description = "The complete reclassification request body.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CompleteReclassificationResponse), Description = "Return results of all reclassification operations in body.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.MultiStatus)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized)]
    [Function("CompleteReclassification")]
    public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "material/{material_id}/reclassify-complete")]
    HttpRequest request, string material_id)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} CompleteReclassification function processed a request.");

            (bool isValid, string? errorMessage, int? hskCaseId) = this.cookieService.ValidateCookies(request);
            if (!isValid)
            {
                return new BadRequestObjectResult(errorMessage);
            }

            if (!int.TryParse(material_id, out int materialId))
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid material_id format. It should be an integer.");
            }

            // Get authorization values
            CmsAuthValues cmsAuthValues = this.GetCmsAuthValues(request);

            _ = int.TryParse(this.cookieService.GetCaseId(request), out int caseId);

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
