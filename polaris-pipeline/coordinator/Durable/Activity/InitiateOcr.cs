using System;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Common.Configuration;
using Common.Extensions;
using Common.Services.BlobStorage;
using Common.Services.OcrService;
using coordinator.Domain;
using coordinator.Durable.Payloads;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Durable.Activity;

public class InitiateOcr
{
    private readonly IPolarisBlobStorageService _polarisBlobStorageService;
    private readonly IOcrService _ocrService;
    private readonly ILogger<InitiateOcr> _logger;

    public InitiateOcr(Func<string, IPolarisBlobStorageService> blobStorageServiceFactory, IOcrService ocrService, IConfiguration configuration, ILogger<InitiateOcr> logger)
    {
        _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
        _ocrService = ocrService.ExceptionIfNull();
        _logger = logger.ExceptionIfNull();
    }

    [Function(nameof(InitiateOcr))]

    public async Task<InitiateOcrResponse> Run([ActivityTrigger] DocumentPayload payload)
    {
        try
        {
            _logger.LogInformation($"Initiate Ocr for {payload.CaseId}, {payload.DocumentId}, {payload.VersionId}");
            var ocrBlobId = new BlobIdType(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Ocr);
            if (await _polarisBlobStorageService.BlobExistsAsync(ocrBlobId))
            {
                return new InitiateOcrResponse { BlobAlreadyExists = true, OcrOperationId = Guid.Empty };
            }

            _logger.LogInformation($"Ocr document for {payload.CaseId}, {payload.DocumentId}, {payload.VersionId}, not found, creating document");
            var pdfBlobId = new BlobIdType(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Pdf);
            await using var documentStream = await _polarisBlobStorageService.GetBlobAsync(pdfBlobId);

            _logger.LogInformation($"pdf document for {payload.CaseId}, {payload.DocumentId}, {payload.VersionId}");
            return new InitiateOcrResponse
            {
                BlobAlreadyExists = false,
                OcrOperationId = await _ocrService.InitiateOperationAsync(documentStream, payload.CorrelationId)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, null);
            throw;
        }
    }
}