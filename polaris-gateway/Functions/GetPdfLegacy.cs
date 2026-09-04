// <copyright file="GetPdf.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Constants;
using Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Services.Artefact;
using PolarisGateway.Services.Artefact.Domain;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class GetPdfLegacy : BaseFunction
{
    private const string PdfContentType = "application/pdf";
    private const string IsOcrProcessedParamName = "isOcrProcessed";
    private const string ForceRefreshParamName = "ForceRefresh";
    private readonly IPdfArtefactService pdfArtefactService;

    public GetPdfLegacy(IPdfArtefactService pdfArtefactService)
        : base()
    {
        this.pdfArtefactService = pdfArtefactService.ExceptionIfNull();
    }

    [Function(nameof(GetPdfLegacy))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetPdfLegacy), tags: ["Documents"], Summary = "Get pdf", Description = "Gives the pdf")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(long), Description = "The document Id (version) of the material", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/pdf", bodyType: typeof(byte[]), Description = "Returns the generated PDF file")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.UnsupportedMediaType, contentType: "application/json", bodyType: typeof(ArtefactResult<Stream>), Description = "Returned when the PDF artefact is not available")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.PdfLegacy)] HttpRequest req, string caseUrn, int caseId, string materialId, long documentId, CancellationToken cancellationToken = default)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var isOcrProcessed = req.Query.ContainsKey(IsOcrProcessedParamName) && bool.Parse(req.Query[IsOcrProcessedParamName]);
        var forceRefresh = req.Query.ContainsKey(ForceRefreshParamName) && bool.Parse(req.Query[ForceRefreshParamName]);

        var request = new GetPdfRequest(
            Urn: caseUrn,
            CaseId: caseId,
            MaterialId: materialId,
            DocumentId: documentId,
            IsOcrProcessed: isOcrProcessed,
            ForceRefresh: forceRefresh);

        var getPdfResult = await this.pdfArtefactService.GetPdfAsync(request, cmsAuthValues, correlationId, isLegacy: true, cancellationToken);

        if (getPdfResult.FileSizeExceedsLimit == true)
        {
            req.HttpContext.Response.Headers.Append(HttpHeaderKeys.AccessControlExposeHeaders, HttpHeaderKeys.CpsFileTooLarge);
            req.HttpContext.Response.Headers[HttpHeaderKeys.CpsFileTooLarge] = "true";
        }

        return getPdfResult.Status == ResultStatus.ArtefactAvailable ?
         new FileStreamResult(getPdfResult.Artefact, PdfContentType) :
         new JsonResult(getPdfResult)
         {
             StatusCode = getPdfResult.FailedHttpStatusCode,
         };
    }
}
