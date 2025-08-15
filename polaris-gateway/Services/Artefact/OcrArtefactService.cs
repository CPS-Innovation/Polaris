using Common.Domain.Ocr;
using Common.Dto.Request.Redaction;
using Common.Services.BlobStorage;
using Common.Services.OcrService;
using PolarisGateway.Services.Artefact.Domain;
using PolarisGateway.Services.Artefact.Factories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Exceptions;
using Common.Mappers;

namespace PolarisGateway.Services.Artefact;
public class OcrArtefactService : IOcrArtefactService
{
    private readonly IArtefactServiceResponseFactory _artefactServiceResponseFactory;
    private readonly ICacheService _cacheService;
    private readonly IOcrService _ocrService;
    private readonly IPdfArtefactService _pdfArtefactService;
    private readonly IRedactionSearchDtoMapper _redactionSearchDtoMapper;

    public OcrArtefactService(
        ICacheService cacheService,
        IArtefactServiceResponseFactory artefactServiceResponseFactory,
        IOcrService ocrService,
        IPdfArtefactService pdfArtefactService, IRedactionSearchDtoMapper redactionSearchDtoMapper)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _artefactServiceResponseFactory = artefactServiceResponseFactory ?? throw new ArgumentNullException(nameof(artefactServiceResponseFactory));
        _ocrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
        _pdfArtefactService = pdfArtefactService ?? throw new ArgumentNullException(nameof(pdfArtefactService));
        _redactionSearchDtoMapper = redactionSearchDtoMapper;
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

    public async Task<IEnumerable<RedactionDefinitionDto>> GetOcrSearchRedactionsAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, string searchTerm, CancellationToken cancellationToken = default)
    {
        var redactionDefinitionDtos = new List<RedactionDefinitionDto>();

        var searchTermList = searchTerm.Split(' ').ToList();

        if (await _cacheService.TryGetJsonObjectAsync<AnalyzeResults>(caseId, documentId, versionId, BlobType.Ocr) is not (true, var results))
            throw new OcrDocumentNotFoundException();

        var toBeRedacted = new List<RedactionSearchDto>();
        var redactionSearchDtos = _redactionSearchDtoMapper.Map(results.ReadResults).ToList();

        for (int i = 0; i < redactionSearchDtos.Count; i++)
        {
            if (!redactionSearchDtos[i].Word.Contains(searchTermList[0], StringComparison.InvariantCultureIgnoreCase))
                continue;

            var potentialRedactions = new List<RedactionSearchDto>(searchTermList.Count) { redactionSearchDtos[i] };
            for (int j = 1; j < searchTermList.Count; j++)
            {
                if (redactionSearchDtos[i + j].Word.Contains(searchTermList[j], StringComparison.InvariantCultureIgnoreCase))
                {
                    potentialRedactions.Add(redactionSearchDtos[i + j]);
                    continue;
                }

                break;
            }

            if (searchTermList.Count != potentialRedactions.Count)
                continue;
            toBeRedacted.AddRange(potentialRedactions);
            i += searchTermList.Count;
        }

        var pageIndexes = toBeRedacted.Select(x => x.PageIndex).Distinct();

        redactionDefinitionDtos.AddRange(pageIndexes
            .Select(pageIndex => new { pageIndex, page = toBeRedacted.First(x => x.PageIndex == pageIndex) })
            .Select(@t => new RedactionDefinitionDto
            {
                PageIndex = @t.pageIndex,
                Width = @t.page.Width,
                Height = @t.page.Height,
                RedactionCoordinates = toBeRedacted.Where(x => x.PageIndex == @t.pageIndex)
                    .Select(x => x.RedactionCoordinates)
                    .ToList()
            }));

        return redactionDefinitionDtos;
    }


}
