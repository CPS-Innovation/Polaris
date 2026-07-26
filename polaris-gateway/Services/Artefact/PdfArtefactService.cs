// <copyright file="PdfArtefactService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Services.Artefact;

using Common.Constants;
using Common.Domain.Ocr;
using Common.Extensions;
using Common.Services.BlobStorage;
using Common.Services.OcrService;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolarisGateway.Models;
using PolarisGateway.Services.Artefact.Domain;
using PolarisGateway.Services.Artefact.Factories;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class PdfArtefactService(
    IOptions<RedactionFileSizeOptions> options,
    ILogger<PdfArtefactService> logger,
    ICacheService cacheService,
    IArtefactServiceResponseFactory artefactServiceResponseFactory,
    IPdfRetrievalService pdfRetrievalService,
    IOcrService ocrService)
    : IPdfArtefactService
{
    private readonly ILogger<PdfArtefactService> logger = logger.ExceptionIfNull();
    private readonly RedactionFileSizeOptions redactionFileSizeOptions = options.Value;
    private readonly IArtefactServiceResponseFactory artefactServiceResponseFactory = artefactServiceResponseFactory.ExceptionIfNull();
    private readonly ICacheService cacheService = cacheService.ExceptionIfNull();
    private readonly IPdfRetrievalService pdfRetrievalService = pdfRetrievalService.ExceptionIfNull();
    private readonly IOcrService ocrService = ocrService.ExceptionIfNull();

    public async Task<ArtefactResult<Stream>> GetPdfAsync(
        GetPdfRequest request,
        string cmsAuthValues,
        Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!request.ForceRefresh && await this.cacheService.TryGetPdfAsync(request.CaseId, request.MaterialId, request.DocumentId, request.IsOcrProcessed) is (true, var stream))
        {
            var cachedFileSizeInMb = await this.cacheService.GetPdfSizeFromMetadataAsync(request.CaseId, request.MaterialId, request.DocumentId, request.IsOcrProcessed);

            return this.ValidateFileSizeAndCreatePdfResult(stream, request.DocumentId, true, cachedFileSizeInMb ?? 0);
        }

        var result = await this.pdfRetrievalService.GetPdfStreamAsync(cmsAuthValues, correlationId, request.Urn, request.CaseId, request.MaterialId, request.DocumentId);

        if (result.Status != PdfConversionStatus.DocumentConverted)
        {
            return this.artefactServiceResponseFactory.CreateFailedResult<Stream>(result.Status, result.FailedStatusCode);
        }

        // Read the PDF into a byte array
        byte[] pdfBytes;
        double fileSizeInMb;

        using (var buffer = new MemoryStream())
        {
            await result.PdfStream.CopyToAsync(buffer);
            pdfBytes = buffer.ToArray();
        }

        // Process OCR and upload to cache
        await this.ProcessAndUploadOcrAsync(pdfBytes, request.CaseId, request.MaterialId, request.DocumentId, correlationId);

        // For PDF upload: use another fresh MemoryStream
        using (var uploadStream = new MemoryStream(pdfBytes))
        {
            fileSizeInMb = Math.Floor((uploadStream.Length / (1024.0 * 1024.0)) * 10) / 10;
            await this.cacheService.UploadPdfAsync(request.CaseId, request.MaterialId, request.DocumentId, request.IsOcrProcessed, uploadStream, fileSizeInMb);
        }

        var (_, pdfStream) = await this.cacheService.TryGetPdfAsync(request.CaseId, request.MaterialId, request.DocumentId, request.IsOcrProcessed);

        return this.ValidateFileSizeAndCreatePdfResult(pdfStream, request.DocumentId, false, fileSizeInMb);
    }

    private ArtefactResult<Stream> ValidateFileSizeAndCreatePdfResult(Stream pdfStream, long documentId, bool fromCache, double fileSizeInMb)
    {
        if (fileSizeInMb > this.redactionFileSizeOptions.FileSizeLimitMb)
        {
            this.logger.LogInformation(
                "Warning: document {DocumentId} has file size {FileSizeMb}MB which exceeds limit {FileSizeLimitMb}MB.",
                documentId,
                fileSizeInMb,
                this.redactionFileSizeOptions.FileSizeLimitMb
            );

            return this.artefactServiceResponseFactory.CreateOkResultWithLargeFileFlag(pdfStream, fromCache, true);
        }

        return this.artefactServiceResponseFactory.CreateOkfResult(pdfStream, fromCache);
    }

    private async Task ProcessAndUploadOcrAsync(byte[] pdfBytes, int caseId, string materialId, long documentId, Guid correlationId)
    {
        // For OCR: use a fresh MemoryStream
        using (var ocrStream = new MemoryStream(pdfBytes))
        {
            var newOcrOperationId = await this.ocrService.InitiateOperationAsync(ocrStream, correlationId);

            const int maxAttempts = 10;
            const int delayMs = 1000;
            int attempt = 0;
            OcrOperationResult ocrResult = null;
            bool ocrSuccess = false;

            while (attempt < maxAttempts)
            {
                ocrResult = await this.ocrService.GetOperationResultsAsync(newOcrOperationId, correlationId);

                if (ocrResult.IsSuccess && ocrResult.AnalyzeResults != null)
                {
                    await this.cacheService.UploadJsonObjectAsync(caseId, materialId, documentId, BlobType.Ocr, ocrResult.AnalyzeResults);
                    ocrSuccess = true;
                    break;
                }

                await Task.Delay(delayMs);
                attempt++;
            }

            if (!ocrSuccess)
            {
                // Log: OCR did not complete successfully, but continue with PDF upload
            }
        }
    }
}
