// <copyright file="RedactDocument.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Functions;

using Common.Configuration;
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Exceptions;
using Common.Extensions;
using Common.Services.BlobStorage;
using coordinator.Clients.PdfRedactor;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Services.CaseUrnResolver;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class RedactDocument
{
    private readonly IValidator<RedactPdfRequestWithDocumentDto> requestValidator;
    private readonly IPdfRedactorClient redactionClient;
    private readonly IPolarisBlobStorageService polarisBlobStorageService;
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly IMdsClient mdsClient;
    private readonly ICaseUrnResolver caseUrnResolver;

    public RedactDocument(
        IValidator<RedactPdfRequestWithDocumentDto> requestValidator,
        IPdfRedactorClient redactionClient,
        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
        IMdsArgFactory mdsArgFactory,
        IConfiguration configuration,
        IMdsClient mdsClient,
        ICaseUrnResolver caseUrnResolver)
    {
        this.requestValidator = requestValidator.ExceptionIfNull();
        this.redactionClient = redactionClient.ExceptionIfNull();
        this.polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty).ExceptionIfNull();
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.mdsClient = mdsClient.ExceptionIfNull();
        this.caseUrnResolver = caseUrnResolver.ExceptionIfNull();
    }

    [Function(nameof(RedactDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RedactDocument)]
        HttpRequest req,
        int caseId,
        string materialId,
        long documentId,
        CancellationToken cancellationToken)
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        var caseUrn = await this.caseUrnResolver.ResolveCaseUrnAsync(caseId, cmsAuthValues, cancellationToken);

        var redactPdfRequest = await req.ReadFromJsonAsync<RedactPdfRequestDto>(cancellationToken);

        using var documentStream = await this.polarisBlobStorageService.GetBlobAsync(new BlobIdType(caseId, materialId, documentId, BlobType.Pdf));

        using var memoryStream = new MemoryStream();
        await documentStream.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();

        Stream redactedDocumentStream = null;

        if (redactPdfRequest.RedactionDefinitions.Count != 0)
        {
            var base64Document = Convert.ToBase64String(bytes);

            var redactionRequest = new RedactPdfRequestWithDocumentDto
            {
                Document = base64Document,
                RedactionDefinitions = redactPdfRequest.RedactionDefinitions,
            };

            var validationResult = await this.requestValidator.ValidateAsync(redactionRequest, cancellationToken);
            if (!validationResult.IsValid)
            {
                throw new BadRequestException(validationResult.FlattenErrors(), nameof(redactPdfRequest));
            }

            redactedDocumentStream = await this.redactionClient.RedactPdfAsync(null, caseId, materialId, documentId, redactionRequest, currentCorrelationId, isLegacy: false);
            if (redactedDocumentStream == null)
            {
                string error = $"Error Saving redaction details to the document for {caseId}, materialId {materialId}";
                throw new InvalidOperationException(error);
            }
        }

        Stream modifiedDocumentStream = null;

        if (redactPdfRequest.DocumentModifications.Count != 0)
        {
            byte[] bytesToModify = null;

            if (redactedDocumentStream != null)
            {
                using var redactedMemoryStream = new MemoryStream();
                await redactedDocumentStream.CopyToAsync(redactedMemoryStream, cancellationToken);
                bytesToModify = redactedMemoryStream.ToArray();
            }
            else
            {
                bytesToModify = bytes;
            }

            var base64DocumentToModify = Convert.ToBase64String(bytesToModify);

            var modificationRequest = new ModifyDocumentWithDocumentDto
            {
                Document = base64DocumentToModify,
                DocumentModifications = redactPdfRequest.DocumentModifications,
                VersionId = redactPdfRequest.VersionId,
            };

            modifiedDocumentStream = await this.redactionClient.ModifyDocument(null, caseId, materialId, documentId, modificationRequest, currentCorrelationId, isLegacy: false);
            if (modifiedDocumentStream == null)
            {
                string error = $"Error modifying document for {caseId}, materialId {materialId}";
                throw new InvalidOperationException(error);
            }
        }

        var arg = this.mdsArgFactory.CreateDocumentVersionArgDto(
            cmsAuthValues.CmsAuthFullValue,
            correlationId: currentCorrelationId,
            caseUrn,
            caseId: caseId,
            DocumentNature.ToNumericDocumentId(materialId, DocumentNature.Types.Document),
            documentId);

        var ddeiResult = await this.mdsClient.UploadPdfAsync(arg, modifiedDocumentStream ?? redactedDocumentStream, cancellationToken);

        if (ddeiResult.StatusCode == HttpStatusCode.Gone || ddeiResult.StatusCode == HttpStatusCode.RequestEntityTooLarge)
        {
            return new StatusCodeResult((int)ddeiResult.StatusCode);
        }

        return new OkResult();
    }
}
