// <copyright file="GetDocumentList.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Dto.Response.Documents;
using Ddei.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using PolarisGateway.Services.MdsOrchestration;
using DdeiClient.Services.CaseUrnResolver;
using Common.Extensions;
using Common.Dto.Request;

public class GetDocumentList : BaseFunction
{
    private readonly IMdsCaseDocumentsOrchestrationService mdsOrchestrationService;
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly ICaseUrnResolver caseUrnResolver;

    public GetDocumentList(
        IMdsCaseDocumentsOrchestrationService mdsOrchestrationService,
        IMdsArgFactory mdsArgFactory,
        ICaseUrnResolver caseUrnResolver)
        : base()
    {
        this.mdsOrchestrationService = mdsOrchestrationService ?? throw new ArgumentNullException(nameof(mdsOrchestrationService));
        this.mdsArgFactory = mdsArgFactory ?? throw new ArgumentNullException(nameof(mdsArgFactory));
        this.caseUrnResolver = caseUrnResolver.ExceptionIfNull();
    }

    [Function(nameof(GetDocumentList))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetDocumentList), tags: ["Documents"], Summary = "Get Document List", Description = "Getting the list of documents")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<DocumentDto>), Summary = "Document List", Description = "Returns list of documennts")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Documents)] HttpRequest req, int caseId, CancellationToken cancellationToken = default)
    {
        var correlationId = EstablishCorrelation(req);
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();
        cancellationToken.ThrowIfCancellationRequested();

        var caseUrn = await this.caseUrnResolver.ResolveCaseUrnAsync(caseId, cmsAuthValues, cancellationToken);

        var arg = this.mdsArgFactory.CreateCaseIdentifiersArg(cmsAuthValues.CmsAuthFullValue, correlationId, caseUrn, caseId);
        var result = await this.mdsOrchestrationService.GetCaseDocuments(arg);

        return new OkObjectResult(result);
    }
}
