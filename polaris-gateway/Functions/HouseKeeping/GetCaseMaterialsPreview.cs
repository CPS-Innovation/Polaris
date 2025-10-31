// <copyright file="GetCaseMaterialsPreview.cs" company="TheCrownProsecutionService">
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
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Common.Dto.Request;
using PolarisGateway.Helpers;
using System;
using System.IO;
using Common.Configuration;
using Common.Constants;

/// <summary>
/// Represents a function that retrieves the case material document for display purposes,
/// intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetCaseMaterialsPreview"/> class.
/// </remarks>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="communicationService">The service used to get inbox communications.</param>
/// <param name="documentService">The service used to process the request and generate the result.</param>
/// <param name="cookieService">The service used to handle cookie-related operations.</param>
public class GetCaseMaterialsPreview(
    ILogger<GetCaseMaterialsPreview> logger,
    ICommunicationService communicationService,
    IDocumentService documentService) : BaseFunction(logger)
{
    private readonly ILogger<GetCaseMaterialsPreview> logger = logger;
    private readonly ICommunicationService communicationService = communicationService;
    private readonly IDocumentService documentService = documentService;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'case-materials/preview' route.
    /// </summary>
    /// <param name="req">The HTTP request.</param>
    /// <param name="caseId">The case Id.</param>
    /// <param name="materialId">The material ID passed as a route parameter.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: nameof(GetCaseMaterialsPreview), tags: ["Material"], Description = "Represents a function that retrieves the case material document for display purposes.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(int), Description = "The material id request parameter.", Required = true)]
    [OpenApiRequestBody("application/json", typeof(FileStreamResult), Description = "Return case summary response.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK)]
    [Function("GetCaseMaterialsPreview")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseMaterialsPreview)] HttpRequest req, int caseId, int materialId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterialsPreview function processed a request.");

            if (caseId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
            }

            if (materialId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid material_id format. It should be an integer.");
            }

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(req);

            // Get the material link
            string link = await this.GetLinkForMaterialAsync(caseId, materialId, cmsAuthValues).ConfigureAwait(true);
            if (string.IsNullOrEmpty(link))
            {
                return new NotFoundObjectResult($"{LoggingConstants.HskUiLogPrefix} No valid link found for the case material document with materialId [{materialId}].");
            }

            // Set caching headers for 30 minutes
            ResponseHeaderHelper.SetCacheHeaders(req.HttpContext.Response, 1800);

            // Set security headers for the response
            ResponseHeaderHelper.SetSecurityHeaders(req.HttpContext.Response);

            return await this.GetMaterialDocumentAsync(caseId.ToString(), link, cmsAuthValues, stopwatch).ConfigureAwait(true);

        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogError($"{ex.Message}");
            return new UnprocessableEntityObjectResult($"{ex.Message}");
        }
        catch (NotSupportedException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} GetCaseMaterialsPreview function encountered unsupported content type.");
            return new UnprocessableEntityObjectResult($"Preview error: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterialsPreview function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }


    /// <summary>
    /// Asynchronously retrieves the communication link for a specific material based on the material ID.
    /// </summary>
    /// <param name="caseId">The ID of the case used to find the associated communication link.</param>
    /// <param name="materialId">The ID of the material whose communication link is being retrieved.</param>
    /// <param name="cmsAuthValues">Authorization values for CMS access.</param>
    /// <returns>
    /// A <see cref="string"/> containing the communication link if found; otherwise, returns null.
    /// </returns>
    private async Task<string> GetLinkForMaterialAsync(int caseId, int materialId, CmsAuthValues cmsAuthValues)
    {
        object linkResult = await this.communicationService.GetCaseMaterialLinkAsync(caseId, materialId, cmsAuthValues).ConfigureAwait(true);

        return linkResult is IActionResult ? null : linkResult as string;
    }

    /// <summary>
    /// Retrieves the material document asynchronously based on the provided link and authorization values.
    /// </summary>
    /// <param name="caseId">The ID of the case associated with the material.</param>
    /// <param name="link">The link to the material document.</param>
    /// <param name="cmsAuthValues">Authorization values for CMS access.</param>
    /// <param name="stopwatch">The stopwatch tracking the function execution time.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that either contains the material document if found, or a file with 'not found' content if the document is missing.
    /// </returns>
    private async Task<IActionResult> GetMaterialDocumentAsync(string caseId, string link, CmsAuthValues cmsAuthValues, Stopwatch stopwatch)
    {
        FileStreamResult downloadedDocument = await this.documentService.GetMaterialDocumentAsync(caseId, link, cmsAuthValues).ConfigureAwait(true);

        if (downloadedDocument == null)
        {
            return new FileStreamResult(new MemoryStream(), "text/plain")
            {
                FileDownloadName = "not_found.txt",
            };
        }

        this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] GetCaseMaterialsPreview function completed in [{stopwatch.Elapsed}]");

        return downloadedDocument;
    }
}
