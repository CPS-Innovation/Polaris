using Common.Constants;
using PolarisGateway.Services.Artefact.Domain;
using PolarisGateway.Services.Artefact.Factories;
using System;
using System.IO;
using System.Threading.Tasks;
using Common.Extensions;

namespace PolarisGateway.Services.Artefact;

public class PdfArtefactService : IPdfArtefactService
{
    private readonly IArtefactServiceResponseFactory _artefactServiceResponseFactory;
    private readonly ICacheService _cacheService;
    private readonly IPdfRetrievalService _pdfRetrievalService;

    public PdfArtefactService(
        ICacheService cacheService,
        IArtefactServiceResponseFactory artefactServiceResponseFactory,
        IPdfRetrievalService pdfRetrievalService)
    {
        _cacheService = cacheService.ExceptionIfNull();
        _artefactServiceResponseFactory = artefactServiceResponseFactory.ExceptionIfNull();
        _pdfRetrievalService = pdfRetrievalService.ExceptionIfNull();
    }

    public async Task<ArtefactResult<Stream>> GetPdfAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, bool forceRefresh = false)
    {
        if (!forceRefresh && await _cacheService.TryGetPdfAsync(caseId, documentId, versionId, isOcrProcessed) is (true, var stream))
        {
            return _artefactServiceResponseFactory.CreateOkfResult(stream, true);
        }

        var result = await _pdfRetrievalService.GetPdfStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId);

        if (result.Status != PdfConversionStatus.DocumentConverted)
        {
            return _artefactServiceResponseFactory.CreateFailedResult<Stream>(result.Status);
        }

        await _cacheService.UploadPdfAsync(caseId, documentId, versionId, isOcrProcessed, result.PdfStream);
        var (_, pdfStream) = await _cacheService.TryGetPdfAsync(caseId, documentId, versionId, isOcrProcessed);
        return _artefactServiceResponseFactory.CreateOkfResult(pdfStream, false);
    }
}
