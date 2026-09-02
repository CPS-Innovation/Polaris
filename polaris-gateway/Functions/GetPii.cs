// <copyright file="GetPii.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Domain.Pii;
using Common.Dto.Request;
using Common.Extensions;
using DdeiClient.Services.CaseUrnResolver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Models;
using PolarisGateway.Services.Artefact;
using PolarisGateway.Services.Artefact.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class GetPii : BaseFunction
{
    private const string TokenQueryParamName = "token";
    private const string IsOcrProcessedParamName = "isOcrProcessed";
    private const string ForceRefreshParamName = "ForceRefresh";
    private readonly IPiiArtefactService piiArtefactService;

    public GetPii(
        IPiiArtefactService piiArtefactService)
        : base()
    {
        this.piiArtefactService = piiArtefactService.ExceptionIfNull();
    }

    [Function(nameof(GetPii))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetPii), tags: ["Documents"], Summary = "Get Pii", Description = "Gives the Pii")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(long), Description = "The document Id (version) of the material", Required = true)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<PiiLine>), Description = "OCR processing completed successfully")]
    [OpenApiResponseWithBody(HttpStatusCode.Accepted, "application/json", typeof(OcrPollResponse), Description = "OCR is still processing. Client should poll using the provided NextUrl")]
    [OpenApiResponseWithBody(HttpStatusCode.UnsupportedMediaType, "application/json", typeof(OcrResult), Description = "OCR processing failed")]
    [OpenApiResponseWithBody(HttpStatusCode.InternalServerError, "application/json", typeof(OcrResult), Description = "Unexpected server error")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Pii)] HttpRequest req, int caseId, string materialId, long documentId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var correlationId = EstablishCorrelation(req);
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        var isOcrProcessed = req.Query.ContainsKey(IsOcrProcessedParamName) && bool.Parse(req.Query[IsOcrProcessedParamName]);
        var forceRefresh = req.Query.ContainsKey(ForceRefreshParamName) && bool.Parse(req.Query[ForceRefreshParamName]);
        var token = req.Query.ContainsKey(TokenQueryParamName) ?
            Guid.Parse(req.Query[TokenQueryParamName]) :
            (Guid?)null;

        var ocrResult = await this.piiArtefactService.GetPiiAsync(cmsAuthValues.CmsAuthFullValue, correlationId, null, caseId, materialId, documentId, isOcrProcessed, token, forceRefresh, isLegacy: false);
        return ocrResult.Status switch
        {
            ResultStatus.ArtefactAvailable => new JsonResult(ocrResult.Artefact),
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
