using Common.Constants;
using Common.Domain.Ocr;
using Common.Extensions;
using Common.Services.BlobStorage;
using Common.Services.OcrService;
using Microsoft.CodeAnalysis.Operations;
using PolarisGateway.Services.Artefact.Domain;
using PolarisGateway.Services.Artefact.Factories;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PolarisGateway.Services.Artefact;

public class PdfArtefactService : IPdfArtefactService
{
    private readonly IArtefactServiceResponseFactory _artefactServiceResponseFactory;
    private readonly ICacheService _cacheService;
    private readonly IPdfRetrievalService _pdfRetrievalService;
    private readonly IOcrService _ocrService;

    public PdfArtefactService(
        ICacheService cacheService,
        IArtefactServiceResponseFactory artefactServiceResponseFactory,
        IPdfRetrievalService pdfRetrievalService,
        IOcrService ocrService)
    {
        _cacheService = cacheService.ExceptionIfNull();
        _artefactServiceResponseFactory = artefactServiceResponseFactory.ExceptionIfNull();
        _pdfRetrievalService = pdfRetrievalService.ExceptionIfNull();
        _ocrService = ocrService.ExceptionIfNull();
    }

    public async Task<ArtefactResult<Stream>> GetPdfAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId, long documentId, bool isOcrProcessed, bool forceRefresh = false)
    {
        if (!forceRefresh && await _cacheService.TryGetPdfAsync(caseId, materialId, documentId, isOcrProcessed) is (true, var stream))
        {
            var sizeInMb1 = await _cacheService.GetPdfSizeAsync(caseId, materialId, documentId, isOcrProcessed);

            if (sizeInMb1 > 0.3)
            {
                return _artefactServiceResponseFactory.CreateOkResultWithLargeFileFlag(stream, true, true);
            }

            return _artefactServiceResponseFactory.CreateOkfResult(stream, true);
        }

        var result = await _pdfRetrievalService.GetPdfStreamAsync(cmsAuthValues, correlationId, urn, caseId, materialId, documentId);

        if (result.Status != PdfConversionStatus.DocumentConverted)
        {
            return _artefactServiceResponseFactory.CreateFailedResult<Stream>(result.Status, result.FailedStatusCode);
        }

        // Read the PDF into a byte array
        byte[] pdfBytes;
        double sizeInMb;
        using (var buffer = new MemoryStream())
        {
            await result.PdfStream.CopyToAsync(buffer);
            pdfBytes = buffer.ToArray();
        }

        // Process OCR and upload to cache
        await ProcessAndUploadOcrAsync(pdfBytes, caseId, materialId, documentId, correlationId);

        // For PDF upload: use another fresh MemoryStream
        using (var uploadStream = new MemoryStream(pdfBytes))
        {
            sizeInMb = uploadStream.Length / (1024.0 * 1024.0);
            await _cacheService.UploadPdfAsync(caseId, materialId, documentId, isOcrProcessed, uploadStream);

            // Save size separately
            await _cacheService.SetPdfSizeAsync(caseId, materialId, documentId, isOcrProcessed, sizeInMb);
        }

        var (_, pdfStream) = await _cacheService.TryGetPdfAsync(caseId, materialId, documentId, isOcrProcessed);


        if (sizeInMb > 0.3) // Assuming 0.3 MB as the threshold for large files
        {
            return _artefactServiceResponseFactory.CreateOkResultWithLargeFileFlag(pdfStream, false, true);
        }

        return _artefactServiceResponseFactory.CreateOkfResult(pdfStream, false);
    }

    private async Task ProcessAndUploadOcrAsync(byte[] pdfBytes, int caseId, string materialId, long documentId, Guid correlationId)
    {
        // For OCR: use a fresh MemoryStream
        using (var ocrStream = new MemoryStream(pdfBytes))
        {
            var newOcrOperationId = await _ocrService.InitiateOperationAsync(ocrStream, correlationId);

            const int maxAttempts = 10;
            const int delayMs = 1000;
            int attempt = 0;
            OcrOperationResult ocrResult = null;
            bool ocrSuccess = false;

            while (attempt < maxAttempts)
            {
                ocrResult = await _ocrService.GetOperationResultsAsync(newOcrOperationId, correlationId);

                if (ocrResult.IsSuccess && ocrResult.AnalyzeResults != null)
                {
                    await _cacheService.UploadJsonObjectAsync(caseId, materialId, documentId, BlobType.Ocr, ocrResult.AnalyzeResults);
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
