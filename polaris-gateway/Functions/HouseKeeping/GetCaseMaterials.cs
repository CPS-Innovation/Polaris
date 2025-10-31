// <copyright file="GetCaseMaterials.cs" company="TheCrownProsecutionService">
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
using Cps.Fct.Hk.Ui.Interfaces.Exceptions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Cps.Fct.Hk.Ui.Interfaces.Enums;
using PolarisGateway.Functions;
using Common.Dto.Response.HouseKeeping;
using System.Collections.Generic;
using System.Linq;
using System;
using PolarisGateway.Helpers;
using Common.Dto.Request;
using Common.Configuration;
using Common.Constants;

/// <summary>
/// Represents a function that retrieves the case materials for a case,
/// intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetCaseMaterials"/> class.
/// </remarks>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="communicationService">The service used to process the request and generate the result.</param>
/// <param name="caseMaterialService">The service used to manage and retrieve case materials.</param>

public class GetCaseMaterials(
    ILogger<GetCaseMaterials> logger,
    ICommunicationService communicationService,
    ICaseMaterialService caseMaterialService): BaseFunction(logger)
{
    private readonly ILogger<GetCaseMaterials> logger = logger;
    private readonly ICommunicationService communicationService = communicationService;
    private readonly ICaseMaterialService caseMaterialService = caseMaterialService;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'case-materials' route.
    /// </summary>
    /// <param name="req">The HTTP request.</param>
    /// <param name="caseId">The case Id.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: nameof(GetCaseMaterials), tags: ["Material"], Description = "Represents a function that retrieves the case materials for a case.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiRequestBody("application/json", typeof(List<CaseMaterial>), Description = "Return case summary response.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<CaseMaterial>), Description = "Return List of case materials.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [Function("GetCaseMaterials")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseMaterials)] HttpRequest req, int caseId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function processed a request.");

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(req);

            // Retrieve case materials
            var (communications, unusedMaterials, usedStatements, usedExhibits, usedMgForms, usedOtherMaterials, exhibitProducers) =
                await this.caseMaterialService.RetrieveCaseMaterialsAsync(caseId, cmsAuthValues).ConfigureAwait(false);

            // Ensure all required data is retrieved before proceeding
            if (communications == null || unusedMaterials == null || usedStatements == null || usedExhibits == null || usedMgForms == null || usedOtherMaterials == null)
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} Failed to retrieve case materials for caseId [{caseId}]");
                throw new UnprocessableEntityException($"Failed to retrieve case materials for caseId [{caseId}]");
            }

            this.logger?.LogInformation(
                $"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] material count: " +
                $"communications [{communications.Count}], " +
                $"unusedMaterials (exhibits) [{unusedMaterials.Exhibits?.Count ?? 0}], " +
                $"unusedMaterials (mgForms) [{unusedMaterials.MgForms?.Count ?? 0}], " +
                $"unusedMaterials (otherMaterials) [{unusedMaterials.OtherMaterials?.Count ?? 0}], " +
                $"unusedMaterials (statements) [{unusedMaterials.Statements?.Count ?? 0}], " +
                $"usedStatements [{usedStatements.Statements?.Count ?? 0}], " +
                $"usedExhibits [{usedExhibits.Exhibits?.Count ?? 0}], " +
                $"usedMgForms [{usedMgForms.MgForms?.Count ?? 0}], " +
                $"usedOtherMaterials [{usedOtherMaterials.MgForms?.Count ?? 0}] " +
                $"exhibitProducers [{exhibitProducers.ExhibitProducers?.Count ?? 0}]");

            // Map and combine communications with attachments
            List<Communication> combinedCommunications =
                await this.GetMappedCommunicationsWithAttachmentsAsync(caseId, communications, cmsAuthValues).ConfigureAwait(false);

            // Map communications to CaseMaterials
            List<CaseMaterial> allCaseMaterials = this.caseMaterialService.MapCommunicationsToCaseMaterials(combinedCommunications) ?? new List<CaseMaterial>();

            // Add Used exhibits
            this.caseMaterialService.AddCaseMaterials(allCaseMaterials!, usedExhibits.Exhibits ?? Enumerable.Empty<Exhibit>(), "Exhibit", "Other Exhibit", "Used");
            if (usedExhibits.Exhibits != null && usedExhibits.Exhibits.Count != 0)
            {
                allCaseMaterials?.AddRange(this.caseMaterialService.MapUsedExhibitsToCaseMaterials(usedExhibits, exhibitProducers, caseId));
            }

            // Add Used statements
            this.caseMaterialService.AddCaseMaterials(allCaseMaterials!, usedStatements.Statements ?? Enumerable.Empty<Statement>(), "Statement", "Other Statement", "Used");
            if (usedStatements.Statements != null && usedStatements.Statements.Count != 0)
            {
                allCaseMaterials?.AddRange(this.caseMaterialService.MapUsedStatementsToCaseMaterials(usedStatements));
            }

            // Add Used MG forms
            this.caseMaterialService.AddCaseMaterials(allCaseMaterials!, usedMgForms.MgForms ?? Enumerable.Empty<MgForm>(), "MG Form", "MG Form", "Used");
            if (usedMgForms.MgForms != null && usedMgForms.MgForms.Count != 0)
            {
                allCaseMaterials?.AddRange(this.caseMaterialService.MapUsedMgFormsToCaseMaterials(usedMgForms));
            }

            // Add Used other materials
            this.caseMaterialService.AddCaseMaterials(allCaseMaterials!, usedOtherMaterials.MgForms ?? Enumerable.Empty<MgForm>(), "Other Material", "Other Material", "Used");
            if (usedOtherMaterials.MgForms != null && usedOtherMaterials.MgForms.Count != 0)
            {
                allCaseMaterials?.AddRange(this.caseMaterialService.MapUsedOtherMaterialsToCaseMaterials(usedOtherMaterials));
            }

            // Add Unused Materials
            foreach (var collection in new List<(string Name, IEnumerable<IMaterial> UnusedMaterials)>
            {
                ("Exhibits", unusedMaterials.Exhibits ?? Enumerable.Empty<Exhibit>()),
                ("MgForms", unusedMaterials.MgForms ?? Enumerable.Empty<MgForm>()),
                ("OtherMaterials", unusedMaterials.OtherMaterials ?? Enumerable.Empty<MgForm>()),
                ("Statements", unusedMaterials.Statements ?? Enumerable.Empty<Statement>()),
            })
            {
                if (collection.UnusedMaterials.Any())
                {
                    this.caseMaterialService.AddCaseMaterials(allCaseMaterials!, collection.UnusedMaterials, collection.Name, "Unused Material", "Unused");
                }
            }

            // Map and add unused materials only if the result is not empty
            List<CaseMaterial> mappedUnusedMaterials = this.caseMaterialService.MapUnusedMaterialsToCaseMaterials(unusedMaterials);
            if (mappedUnusedMaterials != null && mappedUnusedMaterials.Any())
            {
                allCaseMaterials?.AddRange(mappedUnusedMaterials);
            }

            this.logger!.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] GetCaseMaterials function completed in [{stopwatch.Elapsed}]");

            if (allCaseMaterials != null)
            {
                foreach (CaseMaterial allCaseMaterial in allCaseMaterials)
                {
                    allCaseMaterial.ReadStatus = allCaseMaterial.ReadStatus.Equals("Complete", StringComparison.OrdinalIgnoreCase) ? SetMaterialReadStatusType.Read.ToString() : SetMaterialReadStatusType.Unread.ToString();
                }
            }

            var response = new OkObjectResult(allCaseMaterials);
            ConfigureResponseHeaders(req.HttpContext.Response);
            return response;
        }
        catch (UnprocessableEntityException ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function encountered an unprocessable entity error: {ex.Message}");
            return new ObjectResult(ex.Message)
            {
                StatusCode = StatusCodes.Status422UnprocessableEntity,
            };
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Configures the response headers to enhance security and prevent caching.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponse"/> object whose headers are being configured.</param>
    private static void ConfigureResponseHeaders(HttpResponse response)
    {
        ResponseHeaderHelper.SetNoCacheHeaders(response);
        ResponseHeaderHelper.SetSecurityHeaders(response);
    }

    /// <summary>
    /// Asynchronously retrieves a list of all attachments for all the communications.
    /// </summary>
    /// <param name="caseId">The ID of the case.</param>
    /// <param name="communications">A collection of <see cref="Communication"/> objects for which attachments need to be retrieved.</param>
    /// <param name="cmsAuthValues">The authorization values required for the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="Attachment"/> objects.</returns>
    private async Task<List<Attachment>> GetAllAttachmentsAsync(int caseId, IReadOnlyCollection<Communication> communications, CmsAuthValues cmsAuthValues)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Retrieving attachments for all communications ...");
            return await this.communicationService.RetrieveAllAttachmentsAsync(communications, cmsAuthValues).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function encountered an error fetching attachments for caseId [{caseId}]: {ex.Message}");
            throw new UnprocessableEntityException($"GetCaseMaterials function encountered an error fetching attachments for caseId [{caseId}]");
        }
    }

    /// <summary>
    /// Retrieves communications with their associated attachments for a specific case.
    /// </summary>
    /// <param name="caseId">The ID of the case.</param>
    /// <param name="communications">A collection of existing communications.</param>
    /// <param name="cmsAuthValues">Authentication details for accessing the CMS.</param>
    /// <returns>A list of <see cref="Communication"/> objects, including the original communications and those mapped from attachments.</returns>
    private async Task<List<Communication>> GetMappedCommunicationsWithAttachmentsAsync(int caseId, IReadOnlyCollection<Communication> communications, CmsAuthValues cmsAuthValues)
    {
        var combinedCommunications = communications.ToList();
        List<Attachment> allAttachments = await this.GetAllAttachmentsAsync(caseId, communications, cmsAuthValues).ConfigureAwait(false);

        if (allAttachments != null && allAttachments.Count != 0)
        {
            try
            {
                // Map attachments to Communication objects
                List<Communication> attachmentCommunications = this.communicationService.MapAttachmentsToCommunications(allAttachments);

                // Combine original communications with mapped attachment communications
                combinedCommunications.AddRange(attachmentCommunications);
            }
            catch (Exception ex)
            {
                this.logger!.LogError($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function encountered an error mapping attachments to communications for caseId [{caseId}]: {ex.Message}");
                throw new UnprocessableEntityException($"GetCaseMaterials function encountered an error mapping attachments to communications for caseId [{caseId}]");
            }
        }

        return combinedCommunications;
    }
}
