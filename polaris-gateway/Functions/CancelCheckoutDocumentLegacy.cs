// <copyright file="CancelCheckoutDocumentLegacy.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Dto.Request;
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class CancelCheckoutDocumentLegacy : BaseFunction
{
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly IMasterDataServiceClient masterDataServiceClient;

    public CancelCheckoutDocumentLegacy(
        IMdsArgFactory mdsArgFactory,
        IMasterDataServiceClient masterDataServiceClient)
    {
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.masterDataServiceClient = masterDataServiceClient.ExceptionIfNull();
    }

    [Function(nameof(CancelCheckoutDocumentLegacy))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(CancelCheckoutDocumentLegacy), tags: ["Documents"], Summary = "Cancel Checkout", Description = "Returns case information using caseUrl and caseId")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the document which has to be removed", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(long), Description = "The version Id of the document which has to be removed", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AddDocumentNoteRequestDto), Summary = "Case found", Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RestApi.DocumentCheckoutLegacy)] HttpRequest req, string caseUrn, int caseId, string materialId, long documentId, CancellationToken cancellationToken = default)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var mdsDocumentIdAndVersionIdArgDto = this.mdsArgFactory.CreateDocumentVersionArgDto(
                cmsAuthValues: cmsAuthValues,
                correlationId: correlationId,
                urn: caseUrn,
                caseId: caseId,
                materialId: materialId,
                documentId: documentId);

        await this.masterDataServiceClient.CancelCheckoutDocumentAsync(
            mdsDocumentIdAndVersionIdArgDto,
            new CmsAuthValues(cmsAuthValues, correlationId),
            cancellationToken);

        return new OkResult();
    }
}
