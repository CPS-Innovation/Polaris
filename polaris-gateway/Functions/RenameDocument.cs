// <copyright file="RenameDocument.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Extensions;
using Common.Telemetry;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Services.CaseUrnResolver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Helpers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class RenameDocument : BaseFunction
{
    private const string ExhibitClassification = "EXHIBIT";
    private const string StatementClassification = "STATEMENT";

    private readonly ILogger<RenameDocument> logger;
    private readonly IMdsClient mdsClient;
    private readonly ICaseUrnResolver caseUrnResolver;

    public RenameDocument(
        ILogger<RenameDocument> logger,
        IMdsClient mdsClient,
        ICaseUrnResolver caseUrnResolver)
    {
        this.logger = logger.ExceptionIfNull();
        this.mdsClient = mdsClient.ExceptionIfNull();
        this.caseUrnResolver = caseUrnResolver.ExceptionIfNull();
    }

    [Function(nameof(RenameDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(RenameDocument), tags: ["Documents"], Summary = "Rename Document", Description = "Rename Document")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Document rename", Description = "Returns list of document notes")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RenameDocument)] HttpRequest req, int caseId, string materialId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var telemetryEvent = new RenameDocumentRequestEvent(caseId, materialId)
        {
            OperationName = nameof(RenameDocument),
        };

        var correlationId = EstablishCorrelation(req);
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        var caseUrn = await this.caseUrnResolver.ResolveCaseUrnAsync(caseId, cmsAuthValues, cancellationToken);

        try
        {
            telemetryEvent.IsRequestValid = true;
            telemetryEvent.CorrelationId = correlationId;

            var body = await RequestHelper.GetJsonBody<RenameDocumentRequestDto, RenameDocumentRequestValidator>(req);
            var isRequestJsonValid = body.IsValid;
            telemetryEvent.IsRequestJsonValid = isRequestJsonValid;
            telemetryEvent.RequestJson = body.RequestJson;

            if (!isRequestJsonValid)
            {
                this.logger.TrackEvent(telemetryEvent);
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto
            {
                CmsAuthValues = cmsAuthValues.CmsAuthFullValue,
                CorrelationId = correlationId,
                Urn = caseUrn,
                CaseId = caseId,
            };
            var documents = await this.mdsClient.ListDocumentsAsync(mdsCaseIdentifiersArgDto, cancellationToken);
            var documentIdNumber = DocumentNature.ToNumericDocumentId(materialId, DocumentNature.Types.Document);

            var document = documents.SingleOrDefault(x => x.DocumentId == documentIdNumber);

            if (document == null)
            {
                return new NotFoundObjectResult("Document not found");
            }

            var mdsRenameDocumentArgDto = new MdsRenameDocumentArgDto
            {
                CmsAuthValues = cmsAuthValues.CmsAuthFullValue,
                CorrelationId = correlationId,
                Urn = caseUrn,
                CaseId = caseId,
                MaterialId = documentIdNumber,
                DocumentName = body.Value.DocumentName,
            };
            if (string.Equals(document.Classification, ExhibitClassification, StringComparison.InvariantCultureIgnoreCase))
            {
                await this.mdsClient.RenameExhibitAsync(mdsRenameDocumentArgDto, cancellationToken);
            }
            else if (!string.Equals(document.Classification, StatementClassification, StringComparison.InvariantCultureIgnoreCase))
            {
                await this.mdsClient.RenameDocumentAsync(mdsRenameDocumentArgDto, cancellationToken);
            }

            telemetryEvent.IsSuccess = true;
            this.logger.TrackEvent(telemetryEvent);

            return new OkResult();
        }
        catch
        {
            this.logger.TrackEventFailure(telemetryEvent);
            throw;
        }
    }
}
