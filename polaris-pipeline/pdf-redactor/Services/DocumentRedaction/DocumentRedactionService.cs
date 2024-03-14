using System;
using System.Threading.Tasks;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Logging;
using Common.Services.BlobStorageService;
using Microsoft.Extensions.Logging;

namespace pdf_redactor.Services.DocumentRedaction
{
    public class DocumentRedactionService : IDocumentRedactionService
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly IUploadFileNameFactory _uploadFileNameFactory;
        private readonly IRedactionProvider _redactionProvider;
        private readonly ILogger<DocumentRedactionService> _logger;

        public DocumentRedactionService(
            IPolarisBlobStorageService blobStorageService,
            IUploadFileNameFactory uploadFileNameFactory,
            IRedactionProvider redactionProvider,
            ILogger<DocumentRedactionService> logger)
        {
            _polarisBlobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _uploadFileNameFactory = uploadFileNameFactory;
            _redactionProvider = redactionProvider;
            _logger = logger;
        }

        public async Task<RedactPdfResponse> RedactPdfAsync(string caseId, string documentId, RedactPdfRequestDto redactPdfRequest, Guid correlationId)
        {
            try
            {
                using var documentStream = await _polarisBlobStorageService.GetDocumentAsync(redactPdfRequest.FileName, correlationId);

                using var redactedDocumentStream = await _redactionProvider.Redact(documentStream, caseId, documentId, redactPdfRequest, correlationId);

                var uploadFileName = _uploadFileNameFactory.BuildUploadFileName(redactPdfRequest.FileName);
                await _polarisBlobStorageService.UploadDocumentAsync(
                    redactedDocumentStream,
                    uploadFileName,
                    caseId,
                    documentId,
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
                _logger.LogMethodError(correlationId, nameof(RedactPdfAsync), ex.Message, ex);
                return new RedactPdfResponse
                {
                    Succeeded = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<Stream> RedactAsync(string caseId, string documentId, RedactPdfRequestWithDocumentDto redactPdfRequest, Guid correlationId)
        {
            try
            {
                byte[] documentBytes = Convert.FromBase64String(redactPdfRequest.Document);
                using var documentStream = new MemoryStream(documentBytes);

                RedactPdfRequestDto pdfRedact = new RedactPdfRequestDto
                {
                    RedactionDefinitions = redactPdfRequest.RedactionDefinitions,
                    VersionId = redactPdfRequest.VersionId,
                };

                var redactedDocumentStream = await _redactionProvider.Redact(documentStream, caseId, documentId, pdfRedact, correlationId);

                var uploadFileName = _uploadFileNameFactory.BuildUploadFileName(redactPdfRequest.FileName);

                // await _polarisBlobStorageService.UploadDocumentAsync(
                //     redactedDocumentStream,
                //     uploadFileName,
                //     caseId,
                //     documentId,
                //     redactPdfRequest.VersionId.ToString(),
                //     correlationId);


                return redactedDocumentStream;
            }
            catch (Exception ex)
            {
                _logger.LogMethodError(correlationId, nameof(RedactAsync), ex.Message, ex);
                throw;
            }
        }
    }
}