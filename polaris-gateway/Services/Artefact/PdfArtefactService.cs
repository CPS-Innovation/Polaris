using Common.Constants;
using PolarisGateway.Services.Artefact.Domain;
using PolarisGateway.Services.Artefact.Factories;

namespace PolarisGateway.Services.Artefact;
public class PdfArtefactService : IPdfArtefactService
{
    private readonly IArtefactServiceResponseFactory _artefactServiceResponseFactory;
    private readonly ICacheService _cacheService;
    private readonly IPdfRetrievalService _pdfRetrievalService;

    public PdfArtefactService(
        ICacheService cacheService,
        IArtefactServiceResponseFactory artefactServiceResponseFactory,
        IPdfRetrievalService pdfRetrievalService
        )
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _artefactServiceResponseFactory = artefactServiceResponseFactory ?? throw new ArgumentNullException(nameof(artefactServiceResponseFactory));
        _pdfRetrievalService = pdfRetrievalService ?? throw new ArgumentNullException(nameof(pdfRetrievalService));
    }

    public async Task<ArtefactResult<Stream>> GetPdfAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed)
    {
        if (await _cacheService.TryGetPdfAsync(caseId, documentId, versionId, isOcrProcessed) is (true, var stream))
        {
            return _artefactServiceResponseFactory.CreateOkfResult(stream, false);
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
