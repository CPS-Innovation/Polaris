// <copyright file="CheckoutDocumentLegacy.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Extensions;
using Common.Dto.Request;
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

public class CheckoutDocumentLegacy : BaseFunction
{
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly IMasterDataServiceClient masterDataServiceClient;

    public CheckoutDocumentLegacy(
        IMdsArgFactory mdsArgFactory,
        IMasterDataServiceClient masterDataServiceClient)
    {
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.masterDataServiceClient = masterDataServiceClient.ExceptionIfNull();
    }

    [Function(nameof(CheckoutDocumentLegacy))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(CheckoutDocumentLegacy), tags: ["Documents"], Summary = "Checkout", Description = "Checkout Document")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material which has to be checked out", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(long), Description = "The document Id (version) of the material which has to be checked out", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Case found", Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.DocumentCheckoutLegacy)] HttpRequest req, string caseUrn, int caseId, string materialId, long documentId, CancellationToken cancellationToken = default)
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

        await this.masterDataServiceClient.CheckoutDocumentAsync(mdsDocumentIdAndVersionIdArgDto, new CmsAuthValues(cmsAuthValues, correlationId), cancellationToken);

        return new OkResult();
    }
}
