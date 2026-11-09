// <copyright file="BaseGeneratePdf.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Durable.Activity.GeneratePdf;

using Common.Clients.PdfGenerator;
using Common.Configuration;
using Common.Constants;
using Common.Extensions;
using Common.Services.BlobStorage;
using coordinator.Domain;
using coordinator.Durable.Payloads;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

public abstract class BaseGeneratePdf
{
    protected readonly IMdsArgFactory MdsArgFactory;
    protected readonly IMdsClient MdsClient;
    protected readonly IMasterDataServiceClient masterDataServiceClient;
    private readonly IPdfGeneratorClient pdfGeneratorClient;
    private readonly IPolarisBlobStorageService polarisBlobStorageService;

    protected BaseGeneratePdf(
        IMdsArgFactory mdsArgFactory,
        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
        IPdfGeneratorClient pdfGeneratorClient,
        IConfiguration configuration,
        IMdsClient mdsClient,
        IMasterDataServiceClient masterDataServiceClient)
    {
        this.MdsArgFactory = mdsArgFactory;
        this.pdfGeneratorClient = pdfGeneratorClient;
        this.polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
        this.MdsClient = mdsClient.ExceptionIfNull();
        this.masterDataServiceClient = masterDataServiceClient.ExceptionIfNull();
    }

    protected async Task<PdfConversionResponse> Run(DocumentPayload payload)
    {
        var blobId = new BlobIdType(payload.CaseId, payload.MaterialId, payload.DocumentId, BlobType.Pdf);

        if (await this.polarisBlobStorageService.BlobExistsAsync(blobId, payload.IsOcredProcessedPreference))
        {
            return new PdfConversionResponse { BlobAlreadyExists = true, PdfConversionStatus = PdfConversionStatus.DocumentConverted };
        }

        if (payload.FileType == null)
        {
            return new PdfConversionResponse { BlobAlreadyExists = false, PdfConversionStatus = PdfConversionStatus.DocumentTypeUnsupported };
        }

        await using var documentStream = await this.GetDocumentStreamAsync(payload);

        var response = await this.pdfGeneratorClient.ConvertToPdfAsync(
            payload.CorrelationId,
            payload.Urn,
            payload.CaseId,
            payload.MaterialId,
            payload.DocumentId,
            documentStream,
            payload.FileType.Value,
            isLegacy: true); // TODO: Set isLegacy to false when polaris is retired

        if (response.Status != PdfConversionStatus.DocumentConverted)
        {
            return new PdfConversionResponse { BlobAlreadyExists = false, PdfConversionStatus = response.Status };
        }

        await this.polarisBlobStorageService.UploadBlobAsync(response.PdfStream, blobId, payload.IsOcredProcessedPreference);
        await response.PdfStream.DisposeAsync();

        return new PdfConversionResponse { BlobAlreadyExists = false, PdfConversionStatus = PdfConversionStatus.DocumentConverted };
    }

    protected abstract Task<Stream> GetDocumentStreamAsync(DocumentPayload payload);
}
