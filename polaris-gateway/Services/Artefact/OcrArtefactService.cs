using Common.Domain.Ocr;
using Common.Extensions;
using Common.Services.BlobStorage;
using Common.Services.OcrService;
using PolarisGateway.Services.Artefact.Domain;
using PolarisGateway.Services.Artefact.Factories;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PolarisGateway.Services.Artefact;
public class OcrArtefactService : IOcrArtefactService
{
    private readonly IArtefactServiceResponseFactory _artefactServiceResponseFactory;
    private readonly ICacheService _cacheService;
    private readonly IOcrService _ocrService;
    private readonly IPdfArtefactService _pdfArtefactService;

    public OcrArtefactService(
        ICacheService cacheService,
        IArtefactServiceResponseFactory artefactServiceResponseFactory,
        IOcrService ocrService,
        IPdfArtefactService pdfArtefactService)
    {
        _cacheService = cacheService.ExceptionIfNull();
        _artefactServiceResponseFactory = artefactServiceResponseFactory.ExceptionIfNull();
        _ocrService = ocrService.ExceptionIfNull();
        _pdfArtefactService = pdfArtefactService.ExceptionIfNull();
    }

    public async Task<ArtefactResult<AnalyzeResults>> GetOcrAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null)
    {
        if (await _cacheService.TryGetJsonObjectAsync<AnalyzeResults>(caseId, documentId, versionId, BlobType.Ocr) is (true, var results))
        {
            // If we have OCR in blob cache then return it
            return _artefactServiceResponseFactory.CreateOkfResult(results, true);
        }

        if (operationId.HasValue)
        {
            // Otherwise if we are being called with an operationId then we are handling a subsequent polling visit
            var ocrResult = await _ocrService.GetOperationResultsAsync(operationId.Value, correlationId);
            if (ocrResult.IsSuccess)
            {
                // If the OCR operation has completed then cache the results and return them
                await _cacheService.UploadJsonObjectAsync(caseId, documentId, versionId, BlobType.Ocr, ocrResult.AnalyzeResults);
                return _artefactServiceResponseFactory.CreateOkfResult(ocrResult.AnalyzeResults, false);
            }

            // If the OCR operation is still in progress then return the operationId    
            return _artefactServiceResponseFactory.CreateInterimResult<AnalyzeResults>(operationId.Value);
        }

        // If we are being called without an operationId and there is nothing in cache then we need to start a new OCR operation...
        var pdfResult = await _pdfArtefactService.GetPdfAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId, isOcrProcessed);

        if (pdfResult.Status != ResultStatus.ArtefactAvailable)
        {
            // ... which might not be possible if a PDF is unobtainable.
            return _artefactServiceResponseFactory.ConvertNonOkResult<Stream, AnalyzeResults>(pdfResult);
        }

        var newOperationId = await _ocrService.InitiateOperationAsync(pdfResult.Artefact, correlationId);
        return _artefactServiceResponseFactory.CreateInterimResult<AnalyzeResults>(newOperationId);
    }
}