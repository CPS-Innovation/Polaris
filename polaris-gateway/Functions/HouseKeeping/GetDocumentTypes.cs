// <copyright file="GetDocumentTypes.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Functions.Functions;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Aspose.Pdf.Operators;
using Common.Constants;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Functions;

/// <summary>
/// A function used to retrieve all document types associated with a classification group.
/// </summary>
/// <param name="logger">The logger instance used to log warnings and errors.</param>
/// <param name="documentTypeMapper">The service called to retrieve document types.</param>
/// <param name="caseId">The case Id.</param>
public class GetDocumentTypes(
    ILogger<GetDocumentTypes> logger,
    IDocumentTypeMapper documentTypeMapper) : BaseFunction(logger)
{
    private readonly ILogger<GetDocumentTypes> logger = logger;
    private readonly IDocumentTypeMapper documentTypeMapper = documentTypeMapper;

    /// <summary>
    /// The Azure Function that retrieves an HTTP request for the 'document/document-types' route.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="caseId">The case Id.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: "GetDocumentTypes", tags: ["Document"], Description = "Represents a function that retrieves all document types.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(DocumentTypeGroup), Description = "Return success response with body.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized)]
    [Function("GetDocumentTypes")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "document/document-types")] HttpRequest request, int caseId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} GetDocumentTypes function processed a request.");

            if (caseId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
            }

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(request);

            IReadOnlyList<DocumentTypeGroup>? result = this.documentTypeMapper.GetDocumentTypesWithClassificationGroup();

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] GetDocumentTypes function completed in [{stopwatch.Elapsed}]");

            return new OkObjectResult(result);
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetDocumentTypes function encountered an invalid operation error: {ex.Message}");
            return new UnprocessableEntityObjectResult($"{ex.Message}");
        }
        catch (NotSupportedException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} GetDocumentTypes function encountered an unsupported content type error: {ex.Message}");
            return new UnprocessableEntityObjectResult($"GetDocumentTypes error: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} GetDocumentTypes function encountered an unauthorized access error: {ex.Message}");
            return new UnauthorizedObjectResult($"GetDocumentTypes error: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetDocumentTypes function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
