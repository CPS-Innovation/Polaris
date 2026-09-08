// <copyright file="GetOcr.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Services.Artefact;
using PolarisGateway.Services.Artefact.Domain;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Common.Extensions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Common.Domain.Ocr;
using DdeiClient.Services.CaseUrnResolver;
using Common.Dto.Request;

public class GetOcr : BaseFunction
{
    private const string TokenQueryParamName = "token";
    private const string IsOcrProcessedParamName = "isOcrProcessed";
    private const string ForceRefreshParamName = "ForceRefresh";
    private readonly IOcrArtefactService ocrArtefactService;
    private readonly ICaseUrnResolver caseUrnResolver;

    public GetOcr(
        IOcrArtefactService ocrArtefactService,
        ICaseUrnResolver caseUrnResolver)
        : base()
    {
        this.ocrArtefactService = ocrArtefactService.ExceptionIfNull();
        this.caseUrnResolver = caseUrnResolver.ExceptionIfNull();
    }

    [Function(nameof(GetOcr))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetOcr), tags: ["Documents"], Summary = "Artefact Result", Description = "Gives the artefact results")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(long), Description = "The document Id (version) of the material", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ArtefactResult<AnalyzeResults>), Summary = "Artefact Result", Description = "Gives the artefact results")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Ocr)] HttpRequest req, int caseId, string materialId, long documentId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var correlationId = EstablishCorrelation(req);
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        var isOcrProcessed = req.Query.ContainsKey(IsOcrProcessedParamName) && bool.Parse(req.Query[IsOcrProcessedParamName]);
        var forceRefresh = req.Query.ContainsKey(ForceRefreshParamName) && bool.Parse(req.Query[ForceRefreshParamName]);
        var token = req.Query.ContainsKey(TokenQueryParamName) ?
            Guid.Parse(req.Query[TokenQueryParamName]) :
            (Guid?)null;

        var ocrResult = await this.ocrArtefactService.GetOcrAsync(cmsAuthValues.CmsAuthFullValue, correlationId, urn: null, caseId, materialId, documentId, isOcrProcessed, token, forceRefresh, isLegacy: false);
        return ocrResult.Status switch
        {
            ResultStatus.ArtefactAvailable => new JsonResult(ocrResult.Artefact)
            {
                StatusCode = (int)HttpStatusCode.OK,
            },
            ResultStatus.PollWithToken => new JsonResult(new
            {
                NextUrl = $"{req.GetDisplayUrl()}{(req.QueryString.Value.StartsWith('?') ? "&" : "?")}{TokenQueryParamName}={ocrResult.ContinuationToken}",
            })
            {
                StatusCode = (int)HttpStatusCode.Accepted, // the client will understand 202 as a signal to poll again
            },
            ResultStatus.Failed => new JsonResult(ocrResult)
            {
                StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
            },
            _ => new JsonResult(ocrResult) { StatusCode = (int)HttpStatusCode.InternalServerError },
        };
    }
}
