// <copyright file="CheckoutDocument.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Dto.Request;
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class CheckoutDocument : BaseFunction
{
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly IMdsClient mdsClient;
    private readonly ICaseUrnResolver caseUrnResolver;

    public CheckoutDocument(
        IMdsArgFactory mdsArgFactory,
        IMdsClient mdsClient,
        ICaseUrnResolver caseUrnResolver)
    {
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.mdsClient = mdsClient.ExceptionIfNull();
        this.caseUrnResolver = caseUrnResolver.ExceptionIfNull();
    }

    [Function(nameof(CheckoutDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(CheckoutDocument), tags: ["Documents"], Summary = "Checkout", Description = "Checkout Document")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material which has to be checked out", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(long), Description = "The document Id (version) of the material which has to be checked out", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Case found", Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.DocumentCheckout)] HttpRequest req, int caseId, string materialId, long documentId, CancellationToken cancellationToken = default)
    {
        var correlationId = EstablishCorrelation(req);
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        var caseUrn = await this.caseUrnResolver.ResolveCaseUrnAsync(caseId, cmsAuthValues, cancellationToken);

        var mdsDocumentIdAndVersionIdArgDto = this.mdsArgFactory.CreateDocumentVersionArgDto(
                     cmsAuthValues: cmsAuthValues.CmsAuthFullValue,
                     correlationId: correlationId,
                     urn: caseUrn,
                     caseId: caseId,
                     materialId: materialId,
                     documentId: documentId);

        await this.mdsClient.CheckoutDocumentAsync(mdsDocumentIdAndVersionIdArgDto, cancellationToken);
        return new OkResult();
    }
}
