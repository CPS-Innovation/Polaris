using System;
using System.Threading.Tasks;
using Common.Domain.Extensions;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Logging;
using Common.Services.BlobStorageService.Contracts;
using Microsoft.Extensions.Logging;

namespace pdf_generator.Services.DocumentRedaction
{
    public class DocumentRedactionService : IDocumentRedactionService
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly IRedactionProvider _redactionProvider;
        private readonly ILogger<DocumentRedactionService> _logger;

        public DocumentRedactionService(
            IPolarisBlobStorageService blobStorageService,
            IRedactionProvider redactionProvider,
            ILogger<DocumentRedactionService> logger)
        {
            _polarisBlobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _redactionProvider = redactionProvider;
            _logger = logger;
        }

        public async Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequestDto redactPdfRequest, Guid correlationId)
        {
            try
            {
                _logger.LogMethodEntry(correlationId, nameof(RedactPdfAsync), redactPdfRequest.ToJson());

                var documentStream = await _polarisBlobStorageService.GetDocumentAsync(redactPdfRequest.FileName, correlationId);
                var redactedDocumentStream = _redactionProvider.Redact(documentStream, redactPdfRequest, correlationId);

                var uploadFileName = GetUploadFileName(redactPdfRequest.FileName);
                await _polarisBlobStorageService.UploadDocumentAsync(
                    redactedDocumentStream,
                    uploadFileName,
                    redactPdfRequest.CaseId.ToString(),
                    redactPdfRequest.PolarisDocumentId,
                    redactPdfRequest.VersionId.ToString(),
                    correlationId);

                return new RedactPdfResponse
                {
                    Succeeded = true,
                    RedactedDocumentName = uploadFileName
                };
            }
            catch (Exception ex)
            {
                return new RedactPdfResponse
                {
                    Succeeded = false,
                    Message = ex.Message
                };
            }
        }
        private string GetUploadFileName(string fileName)
        {
            var fileNameWithoutExtension = fileName.IndexOf(".pdf", StringComparison.OrdinalIgnoreCase) > -1
                ? fileName.Split(".pdf", StringSplitOptions.RemoveEmptyEntries)[0]
                : fileName;
            return $"{fileNameWithoutExtension}_{DateTime.Now.Ticks.GetHashCode().ToString("x").ToUpper()}.pdf"; //restore save redaction to same storage for now, but with additional randomised identifier
        }
    }
}