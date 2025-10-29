using Common.Configuration;
using Common.Domain.Ocr;
using Common.Dto.Request.Redaction;
using Common.Exceptions;
using Common.Mappers;
using Common.Services.BlobStorage;
using Common.Services.BlobStorage.Factories;
using coordinator.Durable.Payloads;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace coordinator.Durable.Activity;

public class BulkRedactionSearchActivity
{
    private readonly IPolarisBlobStorageService _polarisBlobStorageService;
    private readonly IRedactionSearchDtoMapper _redactionSearchDtoMapper;
    private readonly IBlobTypeIdFactory _blobTypeIdFactory;

    public BulkRedactionSearchActivity(Func<string, IPolarisBlobStorageService> blobStorageServiceFactory, IRedactionSearchDtoMapper redactionSearchDtoMapper, IBlobTypeIdFactory blobTypeIdFactory, IConfiguration configuration)
    {
        _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
        _redactionSearchDtoMapper = redactionSearchDtoMapper;
        _blobTypeIdFactory = blobTypeIdFactory;
    }

    [Function(nameof(BulkRedactionSearchActivity))]
    public async Task<IEnumerable<RedactionDefinitionDto>> Run([ActivityTrigger] BulkRedactionSearchPayload payload)
    {
        var redactionDefinitionDtos = new List<RedactionDefinitionDto>();

        var searchTermList = payload.SearchText.Split(' ').ToList();
        var blobId = _blobTypeIdFactory.CreateBlobId(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Ocr);
        var results = await _polarisBlobStorageService.TryGetObjectAsync<AnalyzeResults>(blobId);
        if (results is null)
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