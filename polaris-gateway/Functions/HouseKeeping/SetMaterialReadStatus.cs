// <copyright file="SetMaterialReadStatus.cs" company="TheCrownProsecutionService">
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
using Cps.Fct.Hk.Ui.Interfaces.Enums;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Common.Constants;
using System.IO;
using System;

/// <summary>
/// Represents a function that sets the material read status as read or unread,
/// intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SetMaterialReadStatus"/> class.
/// </remarks>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="communicationService">The service used to get call rename service.</param>
public class SetMaterialReadStatus(
    ILogger<SetMaterialReadStatus> logger,
    ICommunicationService communicationService) : BaseFunction(logger)
{
    private readonly ILogger<SetMaterialReadStatus> logger = logger;
    private readonly ICommunicationService communicationService = communicationService;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'material/read-status' route.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: "SetMaterialReadStatus", tags: ["Material"], Description = "Represents a function that sets the material read or unread status.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiRequestBody("application/json", typeof(SetMaterialReadStatusRequest), Description = "Body containing the material id and read/unread status where 0 = Invalid, 1 = Read and 2 = Unread as status property.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SetMaterialReadStatusResponse), Description = "Return the material id that marked as read/unread.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.InternalServerError, Description = $"{LoggingConstants.HskUiLogPrefix} SetMaterialReadStatus function encountered an error")]
    [Function("SetMaterialReadStatus")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "material/read-status")] HttpRequest request, int caseId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} SetMaterialReadStatus function processed a request.");

            if (caseId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
            }

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(request);

            string requestBody = await new StreamReader(request.Body).ReadToEndAsync().ConfigureAwait(false);

            SetMaterialReadStatusRequest setMaterialReadStatusRequest = JsonConvert.DeserializeObject<SetMaterialReadStatusRequest>(requestBody);

            if (setMaterialReadStatusRequest == null)
            {
                return new BadRequestObjectResult(nameof(setMaterialReadStatusRequest));
            }

            if (setMaterialReadStatusRequest?.state == Common.Enums.MaterialReadStatusType.Invalid)
            {
                return new BadRequestObjectResult(nameof(SetMaterialReadStatusRequest.state));
            }

            if (setMaterialReadStatusRequest?.materialId == 0)
            {
                return new BadRequestObjectResult(nameof(SetMaterialReadStatusRequest.materialId));
            }

            SetMaterialReadStatusResponse? result = await this.communicationService.SetMaterialReadStatusAsync(setMaterialReadStatusRequest!.materialId, setMaterialReadStatusRequest.state, cmsAuthValues).ConfigureAwait(true);

            if (result?.CompleteCommunicationData?.Id == null)
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] and Material id [{setMaterialReadStatusRequest.materialId}] SetMaterialReadStatus function failed in [{stopwatch.Elapsed}]");
                return new UnprocessableEntityObjectResult(result);
            }

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] and Material id [{setMaterialReadStatusRequest.materialId}] SetMaterialReadStatus function completed in [{stopwatch.Elapsed}]");

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
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} SetMaterialReadStatus function encountered unsupported content type.");
            return new UnprocessableEntityObjectResult($"Rename error: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} SetMaterialReadStatus function encountered UnauthorizedAccess Exception.");
            return new UnauthorizedObjectResult($"Rename error: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} SetMaterialReadStatus function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
