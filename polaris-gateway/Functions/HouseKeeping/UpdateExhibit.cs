// <copyright file="UpdateExhibit.cs" company="TheCrownProsecutionService">
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
using Cps.Fct.Hk.Ui.Services.Validators;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Common.Constants;
using Common.Dto.Request;
using System.IO;
using Common.Configuration;
using System;

/// <summary>
/// Represents a function that updates exhibit.
/// </summary>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="communicationService">The service used to process the request and generate the result.</param>
/// <param name="cookieService">The service used to handle cookie-related operations.</param>
/// <param name="requestValidator">The fluent validation validator for update exhibit request body.</param>
public class UpdateExhibit(
     ILogger<UpdateExhibit> logger,
     ICommunicationService communicationService,
     UpdateExhibitRequestValidator requestValidator) : BaseFunction(logger)
{
    private readonly ILogger<UpdateExhibit> logger = logger;
    private readonly ICommunicationService communicationService = communicationService;
    private readonly UpdateExhibitRequestValidator requestValidator = requestValidator;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'exhibit/update' route.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: "UpdateExhibit", tags: ["Exhibit"], Description = "Represents a function that updates an exhibit.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(UpdateExhibitRequest), Required = true, Description = "The update exhibit request body.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UpdateExhibitResponse), Description = "Return success response with body.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized)]
    [Function("UpdateExhibit")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = RestApi.UpdateExhibit)] HttpRequest request, int caseId, int materialId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} UpdateExhibit function processed a request.");

            if (caseId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
            }

            if (materialId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
            }

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(request);

            string requestBody = await new StreamReader(request.Body).ReadToEndAsync().ConfigureAwait(false);

            UpdateExhibitRequest updateExhibitRequest = JsonConvert.DeserializeObject<UpdateExhibitRequest>(requestBody);

            if (updateExhibitRequest is null)
            {
                return new BadRequestObjectResult("Exhibit request is null");
            }

            updateExhibitRequest.MaterialId = materialId;
            updateExhibitRequest.CaseId = caseId;

            FluentValidation.Results.ValidationResult validationResult = this.requestValidator.Validate(updateExhibitRequest);
            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(validationResult.Errors.ToArray());
            }

            UpdateExhibitResponse result = await this.communicationService.UpdateExhibitAsync(
                caseId,
                updateExhibitRequest,
                cmsAuthValues).ConfigureAwait(true);

            if (result?.UpdateExhibitData?.Id == null)
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] UpdateExhibit function failed in [{stopwatch.Elapsed}]");
                return new UnprocessableEntityObjectResult(result);
            }

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] UpdateExhibit function completed in [{stopwatch.Elapsed}]");

            var response = new OkObjectResult(result);

            return response;
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} UpdateExhibit function encountered an invalid operation error: {ex.Message}");
            return new UnprocessableEntityObjectResult($"{ex.Message}");
        }
        catch (NotSupportedException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} UpdateExhibit function encountered an unsupported content type error: {ex.Message}");
            return new UnprocessableEntityObjectResult($"Update exhibit error: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} UpdateExhibit function encountered an unauthorized access error: {ex.Message}");
            return new UnauthorizedObjectResult($"Update exhibit error: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} UpdateExhibit function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
