// <copyright file="UpdateStatement.cs" company="TheCrownProsecutionService">
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
using System.IO;
using System;
using Common.Configuration;

/// <summary>
/// Represents a function that updates a statement.
/// </summary>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="communicationService">The service used to process the request and generate the result.</param>
/// <param name="cookieService">The service used to handle cookie-related operations.</param>
/// <param name="requestValidator">The fluent validation validator for update statement request body.</param>
public class UpdateStatement(
    ILogger<UpdateStatement> logger,
    ICommunicationService communicationService,
    UpdateStatementRequestValidator requestValidator) : BaseFunction(logger)
{
    private readonly ILogger<UpdateStatement> logger = logger;
    private readonly ICommunicationService communicationService = communicationService;
    private readonly UpdateStatementRequestValidator requestValidator = requestValidator;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'statement/update' route.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: "UpdateStatement", tags: ["Statement"], Description = "Represents a function that updates a statement.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(UpdateStatementRequest), Required = true, Description = "The update statement request body.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UpdateStatementResponse), Description = "Return success response with body.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized)]
    [Function("UpdateStatement")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = RestApi.UpdateStatement)] HttpRequest request, int caseId, int materialId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} UpdateStatement function processed a request.");

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

            var updateStatementRequest = JsonConvert.DeserializeObject<UpdateStatementRequest>(requestBody);
            updateStatementRequest.MaterialId = materialId;
            
            if (updateStatementRequest is null)
            {
                return new BadRequestObjectResult("Statement request is null");
            }

            FluentValidation.Results.ValidationResult validationResult = this.requestValidator.Validate(updateStatementRequest);
            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(validationResult.Errors.ToArray());
            }

            UpdateStatementResponse result = await this.communicationService.UpdateStatementAsync(
                caseId,
                updateStatementRequest,
                cmsAuthValues).ConfigureAwait(true);

            if (result?.UpdateStatementData?.Id == null)
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] UpdateStatement function failed in [{stopwatch.Elapsed}]");
                return new UnprocessableEntityObjectResult(result);
            }

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] UpdateStatement function completed in [{stopwatch.Elapsed}]");

            var response = new OkObjectResult(result);

            return response;
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} UpdateStatement function encountered an invalid operation error: {ex.Message}");
            return new UnprocessableEntityObjectResult($"{ex.Message}");
        }
        catch (NotSupportedException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} UpdateStatement function encountered an unsupported content type error: {ex.Message}");
            return new UnprocessableEntityObjectResult($"Update statement error: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} UpdateStatement function encountered an unauthorized access error: {ex.Message}");
            return new UnauthorizedObjectResult($"Update statement error: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} UpdateStatement function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
