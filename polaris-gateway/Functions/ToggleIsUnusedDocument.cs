// <copyright file="ToggleIsUnusedDocument.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Extensions;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Domain.Args;
using DdeiClient.Services.CaseUrnResolver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class ToggleIsUnusedDocument : BaseFunction
{
    private readonly IMdsClient mdsClient;
    private readonly ICaseUrnResolver caseUrnResolver;

    public ToggleIsUnusedDocument(
        IMdsClient mdsClient,
        ICaseUrnResolver caseUrnResolver)
    {
        this.mdsClient = mdsClient.ExceptionIfNull();
        this.caseUrnResolver = caseUrnResolver.ExceptionIfNull();
    }

    [Function(nameof(ToggleIsUnusedDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(ToggleIsUnusedDocument), tags: ["Documents"], Summary = "Toggle Is Unused Document", Description = "Toggle Is Unused Document")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material", Required = true)]
    [OpenApiParameter("isUnused", In = ParameterLocation.Path, Type = typeof(string), Description = "Is un used document", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Document Note List", Description = "Returns list of document notes")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ToggleIsUnusedDocument)] HttpRequest req,
        int caseId,
        string materialId,
        string isUnused,
        CancellationToken cancellationToken = default)
    {
        var correlationId = EstablishCorrelation(req);
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        var caseUrn = await this.caseUrnResolver.ResolveCaseUrnAsync(caseId, cmsAuthValues, cancellationToken);

        var toggleIsUnusedDocumentDto = new MdsToggleIsUnusedDocumentDto
        {
            CaseId = caseId,
            CmsAuthValues = cmsAuthValues.CmsAuthFullValue,
            CorrelationId = correlationId,
            MaterialId = DocumentNature.ToNumericDocumentId(materialId, DocumentNature.Types.Document),
            IsUnused = isUnused,
            Urn = caseUrn,
        };

        cancellationToken.ThrowIfCancellationRequested();

        return await this.mdsClient.ToggleIsUnusedDocumentAsync(toggleIsUnusedDocumentDto, cancellationToken) ? new OkResult() : new BadRequestResult();
    }
}
