// <copyright file="GetDocumentNotes.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Dto.Request;
using Common.Dto.Response.Document;
using Common.Extensions;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Services.CaseUrnResolver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class GetDocumentNotes : BaseFunction
{
    private readonly IMdsClient mdsClient;
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly ICaseUrnResolver caseUrnResolver;

    public GetDocumentNotes(
        IMdsClient mdsClient,
        IMdsArgFactory mdsArgFactory,
        ICaseUrnResolver caseUrnResolver)
        : base()
    {
        this.mdsClient = mdsClient.ExceptionIfNull();
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.caseUrnResolver = caseUrnResolver.ExceptionIfNull();
    }

    [Function(nameof(GetDocumentNotes))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetDocumentNotes), tags: ["Documents"], Summary = "Get Document Note List", Description = "Getting the list of document notes")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<DocumentNoteDto>), Summary = "Document Note List", Description = "Returns list of document notes")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.DocumentNotes)] HttpRequest req, int caseId, string materialId, CancellationToken cancellationToken = default)
    {
        var correlationId = EstablishCorrelation(req);
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        var caseUrn = await this.caseUrnResolver.ResolveCaseUrnAsync(caseId, cmsAuthValues, cancellationToken);

        var arg = this.mdsArgFactory.CreateDocumentArgDto(cmsAuthValues.CmsAuthFullValue, correlationId, caseUrn, caseId, materialId);

        var result = await this.mdsClient.GetDocumentNotesAsync(arg, cancellationToken: cancellationToken);

        return new OkObjectResult(result);
    }
}
