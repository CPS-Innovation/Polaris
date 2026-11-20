// <copyright file="ModifyDocumentLegacy.cs" company="TheCrownProsecutionService">
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
using DdeiClient.Enums;
using DdeiClient.Factories;
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

public class ModifyDocumentLegacy
{
    private readonly IValidator<ModifyDocumentWithDocumentDto> requestValidator;
    private readonly IPdfRedactorClient pdfRedactorClient;
    private readonly IPolarisBlobStorageService polarisBlobStorageService;
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly IMasterDataServiceClient masterDataServiceClient;

    public ModifyDocumentLegacy(
        IValidator<ModifyDocumentWithDocumentDto> requestValidator,
        IPdfRedactorClient pdfRedactorClient,
        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
        IMdsArgFactory mdsArgFactory,
        IConfiguration configuration,
        IMasterDataServiceClient masterDataServiceClient)
    {
        this.requestValidator = requestValidator.ExceptionIfNull();
        this.pdfRedactorClient = pdfRedactorClient.ExceptionIfNull();
        this.polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty).ExceptionIfNull();
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.masterDataServiceClient = masterDataServiceClient.ExceptionIfNull();
    }

    [Function(nameof(ModifyDocumentLegacy))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ModifyDocumentLegacy)]
        HttpRequest req,
        string caseUrn,
        int caseId,
        string materialId,
        long documentId)
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();

        var modifyDocumentRequest = await req.ReadFromJsonAsync<ModifyDocumentRequestDto>();

        await using var documentStream = await this.polarisBlobStorageService.GetBlobAsync(new BlobIdType(caseId, materialId, documentId, BlobType.Pdf));

        using var memoryStream = new MemoryStream();
        await documentStream.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();

        var base64Document = Convert.ToBase64String(bytes);

        var modificationRequest = new ModifyDocumentWithDocumentDto
        {
            Document = base64Document,
            DocumentModifications = modifyDocumentRequest.DocumentModifications,
            VersionId = modifyDocumentRequest.VersionId,
        };

        var validationResult = await this.requestValidator.ValidateAsync(modificationRequest);
        if (!validationResult.IsValid)
        {
            throw new BadRequestException(validationResult.FlattenErrors(), nameof(modificationRequest));
        }

        await using var modifiedDocumentStream = await this.pdfRedactorClient.ModifyDocument(caseUrn, caseId, materialId, documentId, modificationRequest, currentCorrelationId, isLegacy: true);
        if (modifiedDocumentStream == null)
        {
            var error = $"Error modifying document for {caseId}, materialId {materialId}";
            throw new Exception(error);
        }

        var cmsAuthValues = req.Headers.GetCmsAuthValues();
        var arg = this.mdsArgFactory.CreateDocumentVersionArgDto(
            cmsAuthValues,
            currentCorrelationId,
            caseUrn,
            caseId,
            DocumentNature.ToNumericDocumentId(materialId, DocumentNature.Types.Document),
            documentId);

        try
        {
            await this.masterDataServiceClient.UploadPdfAsync(
                arg,
                modifiedDocumentStream,
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
