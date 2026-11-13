// <copyright file="GetDocumentNotesLegacy.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Dto.Request;
using Common.Dto.Response.Document;
using Common.Extensions;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
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

public class GetDocumentNotesLegacy : BaseFunction
{
    private readonly IMasterDataServiceClient masterDataServiceClient;
    private readonly IMdsArgFactory mdsArgFactory;

    public GetDocumentNotesLegacy(
        IMasterDataServiceClient masterDataServiceClient,
        IMdsArgFactory mdsArgFactory)
        : base()
    {
        this.masterDataServiceClient = masterDataServiceClient.ExceptionIfNull();
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
    }

    [Function(nameof(GetDocumentNotesLegacy))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetDocumentNotesLegacy), tags: ["Documents"], Summary = "Get Document Note List", Description = "Getting the list of document notes")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<DocumentNoteDto>), Summary = "Document Note List", Description = "Returns list of document notes")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.DocumentNotesLegacy)] HttpRequest req, string caseUrn, int caseId, string materialId, CancellationToken cancellationToken = default)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = this.mdsArgFactory.CreateDocumentArgDto(cmsAuthValues, correlationId, caseUrn, caseId, materialId);

        var result = await this.masterDataServiceClient.ListDocumentNotesAsync(arg, new CmsAuthValues(cmsAuthValues, correlationId),  cancellationToken);

        return new OkObjectResult(result);
    }
}
