// <copyright file="GenerateThumbnail.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Dto.Request;
using Common.Extensions;
using DdeiClient.Services.CaseUrnResolver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Clients.PdfThumbnailGenerator;
using PolarisGateway.Extensions;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class GenerateThumbnail : BaseFunction
{
    private readonly IPdfThumbnailGeneratorClient pdfThumbnailGeneratorClient;

    public GenerateThumbnail(
        IPdfThumbnailGeneratorClient pdfThumbnailGeneratorClient)
        : base()
    {
        this.pdfThumbnailGeneratorClient = pdfThumbnailGeneratorClient ?? throw new ArgumentNullException(nameof(pdfThumbnailGeneratorClient));
    }

    [Function(nameof(GenerateThumbnail))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    [OpenApiOperation(operationId: nameof(GenerateThumbnail), tags: ["Documents"], Summary = "Generate Thumbnail", Description = "Generate Thumbnail")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(long), Description = "The document Id (version) of the material", Required = true)]
    [OpenApiParameter("maxDimensionPixel", In = ParameterLocation.Path, Type = typeof(int), Description = "The max Dimension Pixel in the document to generate thumbnail", Required = true)]
    [OpenApiParameter("pageIndex", In = ParameterLocation.Path, Type = typeof(int), Description = "The page Index of the document to generate thumbnail", Required = false)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AddDocumentNoteRequestDto), Summary = "Case found", Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.GenerateThumbnail)] HttpRequest req,
        int caseId,
        string materialId,
        int documentId,
        int maxDimensionPixel,
        int? pageIndex,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var correlationId = EstablishCorrelation(req);
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        return await (await this.pdfThumbnailGeneratorClient.GenerateThumbnailAsync(caseUrn: null, caseId, materialId, documentId, maxDimensionPixel, pageIndex, cmsAuthValues.CmsAuthFullValue, correlationId, isLegacy: false)).ToActionResult();
    }
}
