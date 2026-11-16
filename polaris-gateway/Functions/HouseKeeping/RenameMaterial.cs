// <copyright file="RenameMaterial.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions.HouseKeeping;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces;
using System.Diagnostics;
using Cps.Fct.Hk.Ui.Services.Validators;
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
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Extensions;
using Common.Mappers;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Services.CaseUrnResolver;
using PolarisGateway.Helpers;
using System.Linq;

/// <summary>
/// Represents a function that renames the material name/subject,
/// intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RenameMaterial"/> class.
/// </remarks>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="communicationService">The service used to get call rename service.</param>
/// <param name="renameMaterialRequestValidator">Request validator.</param>
public class RenameMaterial(
    ILogger<RenameMaterial> logger,
    ICommunicationService communicationService,
    RenameMaterialRequestValidator renameMaterialRequestValidator,
    IMdsClient? mdsClient = null,
    IMasterDataServiceClient? masterDataServiceClient = null,
    ICmsDocumentDtoMapper? cmsDocumentDtoMapper = null,
    ICaseUrnResolver? caseUrnResolver = null) : BaseFunction(logger)
{
    private readonly ILogger<RenameMaterial> logger = logger;
    private readonly ICommunicationService communicationService = communicationService;
    private readonly RenameMaterialRequestValidator renameMaterialRequestValidator = renameMaterialRequestValidator;
    private readonly IMdsClient? mdsClient = mdsClient;
    private readonly IMasterDataServiceClient? masterDataServiceClient = masterDataServiceClient;
    private readonly ICmsDocumentDtoMapper? cmsDocumentDtoMapper = cmsDocumentDtoMapper;
    private readonly ICaseUrnResolver? caseUrnResolver = caseUrnResolver;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'material/rename' route.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="caseId">The case that the material belong to.</param>
    /// <param name="materialId">The material to rename.</param>
    /// <returns>An <see cref="IActionResult"/> representing the response of the function.</returns>
    [OpenApiOperation(operationId: "RenameMaterial", tags:["Material"], Description = "Represents a function that renames the material name/subject.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(RenameMaterialRequest), Required = true, Description = "The rename material request body.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RenameMaterialResponse), Description = "Return success response with body.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized)]
    [Function("RenameMaterial")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = RestApi.RenameMaterial)] HttpRequest request, int caseId, int materialId, CancellationToken cancellationToken = default)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} RenameMaterial function processed a request.");

            if (caseId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
            }

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(request);

            request.Headers.ContentType = "application/json";
            RenameMaterialRequest? renameMaterialRequest;
            try
            {
                renameMaterialRequest = await request.ReadFromJsonAsync<RenameMaterialRequest>(cancellationToken).ConfigureAwait(false);
            }
            catch (System.Text.Json.JsonException ex)
            {
                this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} RenameMaterial function received invalid JSON.");
                return new BadRequestObjectResult("Invalid JSON request body.");
            }

            if (renameMaterialRequest is null)
            {
                return new BadRequestObjectResult("A rename material request is required.");
            }

            FluentValidation.Results.ValidationResult validationResult = await this.renameMaterialRequestValidator.ValidateAsync(renameMaterialRequest, cancellationToken).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(validationResult.Errors.ToArray());
            }

            RenameMaterialResponse result;
            if (this.mdsClient is not null && this.masterDataServiceClient is not null && this.cmsDocumentDtoMapper is not null && this.caseUrnResolver is not null)
            {
                result = await this.RenameDocumentMaterialAsync(caseId, materialId, renameMaterialRequest, cmsAuthValues, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                result = await this.communicationService.RenameMaterialAsync(caseId, renameMaterialRequest.materialId, renameMaterialRequest.subject, cmsAuthValues, renameMaterialRequest.id, cancellationToken).ConfigureAwait(false);
            }

            if (result is null)
            {
                return new NotFoundObjectResult("Document not found");
            }

            if (result?.RenameMaterialData?.Id == null)
            {
                this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] RenameMaterial function failed in [{stopwatch.Elapsed}]");
                return new UnprocessableEntityObjectResult(result);
            }

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] RenameMaterial function completed in [{stopwatch.Elapsed}]");

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
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} RenameMaterial function encountered unsupported content type.");
            return new UnprocessableEntityObjectResult($"Rename error: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} RenameMaterial function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    private async Task<RenameMaterialResponse> RenameDocumentMaterialAsync(
        int caseId,
        int materialId,
        RenameMaterialRequest renameMaterialRequest,
        CmsAuthValues cmsAuthValues,
        CancellationToken cancellationToken)
    {
        string caseUrn = await this.caseUrnResolver!.ResolveCaseUrnAsync(caseId, cmsAuthValues, cancellationToken).ConfigureAwait(false);
        var caseIdentifiers = new MdsCaseIdentifiersArgDto
        {
            CmsAuthValues = cmsAuthValues.CmsAuthFullValue,
            CorrelationId = renameMaterialRequest.id,
            Urn = caseUrn,
            CaseId = caseId,
        };

        var documentsResponse = await this.masterDataServiceClient!.ListDocumentsAsync(
            caseIdentifiers,
            new CmsAuthValues(cmsAuthValues.CmsAuthFullValue, renameMaterialRequest.id),
            cancellationToken).ConfigureAwait(false);
        var documentId = DocumentNature.ToNumericDocumentId(materialId.ToString(), DocumentNature.Types.Document);
        var document = documentsResponse
            .Select(x => this.cmsDocumentDtoMapper!.Map(x, null))
            .SingleOrDefault(x => x.DocumentId == documentId);

        if (document is null)
        {
            return null;
        }

        var renameArguments = new MdsRenameDocumentArgDto
        {
            CmsAuthValues = cmsAuthValues.CmsAuthFullValue,
            CorrelationId = renameMaterialRequest.id,
            Urn = caseUrn,
            CaseId = caseId,
            MaterialId = documentId,
            DocumentName = renameMaterialRequest.subject,
        };

        int renamedMaterialId = (int)documentId;
        if (string.Equals(document.Classification, "EXHIBIT", StringComparison.InvariantCultureIgnoreCase))
        {
            renamedMaterialId = (await this.mdsClient!.RenameExhibitAsync(renameArguments, cancellationToken).ConfigureAwait(false))?.Id ?? 0;
        }
        else if (!string.Equals(document.Classification, "STATEMENT", StringComparison.InvariantCultureIgnoreCase))
        {
            renamedMaterialId = (await this.mdsClient!.RenameDocumentAsync(renameArguments, cancellationToken).ConfigureAwait(false))?.Id ?? 0;
        }

        return new RenameMaterialResponse(new RenameMaterialData { Id = renamedMaterialId });
    }
}
