// <copyright file="RedactDocumentLegacy.cs" company="TheCrownProsecutionService">
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
using Cps.MasterDataService.Infrastructure.ApiClient;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
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
using Stream = System.IO.Stream;

public class RedactDocumentLegacy
{
    private readonly IValidator<RedactPdfRequestWithDocumentDto> requestValidator;
    private readonly IPdfRedactorClient redactionClient;
    private readonly IPolarisBlobStorageService polarisBlobStorageService;
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly IMasterDataServiceClient masterDataServiceClient;

    public RedactDocumentLegacy(
        IValidator<RedactPdfRequestWithDocumentDto> requestValidator,
        IPdfRedactorClient redactionClient,
        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
        IMdsArgFactory mdsArgFactory,
        IConfiguration configuration,
        IMasterDataServiceClient masterDataServiceClient)
    {
        this.requestValidator = requestValidator.ExceptionIfNull();
        this.redactionClient = redactionClient.ExceptionIfNull();
        this.polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty).ExceptionIfNull();
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.masterDataServiceClient = masterDataServiceClient.ExceptionIfNull();
    }

    [Function(nameof(RedactDocumentLegacy))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RedactDocumentLegacy)]
        HttpRequest req,
        string caseUrn,
        int caseId,
        string materialId,
        long documentId)
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();

        var redactPdfRequest = await req.ReadFromJsonAsync<RedactPdfRequestDto>();

        using var documentStream = await this.polarisBlobStorageService.GetBlobAsync(new BlobIdType(caseId, materialId, documentId, BlobType.Pdf));

        using var memoryStream = new MemoryStream();
        await documentStream.CopyToAsync(memoryStream);
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

            var validationResult = await this.requestValidator.ValidateAsync(redactionRequest);
            if (!validationResult.IsValid)
            {
                throw new BadRequestException(validationResult.FlattenErrors(), nameof(redactPdfRequest));
            }

            redactedDocumentStream = await this.redactionClient.RedactPdfAsync(caseUrn, caseId, materialId, documentId, redactionRequest, currentCorrelationId, isLegacy: true);
            if (redactedDocumentStream == null)
            {
                string error = $"Error Saving redaction details to the document for {caseId}, materialId {materialId}";
                throw new Exception(error);
            }
        }

        Stream modifiedDocumentStream = null;

        if (redactPdfRequest.DocumentModifications.Count != 0)
        {
            byte[] bytesToModify = null;

            if (redactedDocumentStream != null)
            {
                using var redactedMemoryStream = new MemoryStream();
                await redactedDocumentStream.CopyToAsync(redactedMemoryStream);
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

            modifiedDocumentStream = await this.redactionClient.ModifyDocument(caseUrn, caseId, materialId, documentId, modificationRequest, currentCorrelationId);
            if (modifiedDocumentStream == null)
            {
                string error = $"Error modifying document for {caseId}, materialId {materialId}";
                throw new Exception(error);
            }
        }

        var cmsAuthValues = req.Headers.GetCmsAuthValues();
        var arg = this.mdsArgFactory.CreateDocumentVersionArgDto(
            cmsAuthValues,
            correlationId: currentCorrelationId,
            caseUrn,
            caseId: caseId,
            DocumentNature.ToNumericDocumentId(materialId, DocumentNature.Types.Document),
            documentId);

        try
        {
            await this.masterDataServiceClient.UploadPdfAsync(
                arg,
                modifiedDocumentStream ?? redactedDocumentStream,
                new CmsAuthValues(cmsAuthValues, currentCorrelationId));

            return new OkResult();
        }
        catch (ApiException ex) when (
            ex.StatusCode is
                (int)HttpStatusCode.Gone or
                (int)HttpStatusCode.RequestEntityTooLarge)
        {
            return new StatusCodeResult(ex.StatusCode);
        }
    }
}
