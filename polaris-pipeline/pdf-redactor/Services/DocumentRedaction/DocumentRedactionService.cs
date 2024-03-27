using Common.Dto.Request;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace pdf_redactor.Services.DocumentRedaction
{
    public class DocumentRedactionService : IDocumentRedactionService
    {
        private readonly IRedactionProvider _redactionProvider;
        private readonly ILogger<DocumentRedactionService> _logger;

        public DocumentRedactionService(
            IRedactionProvider redactionProvider,
            ILogger<DocumentRedactionService> logger)
        {
            _redactionProvider = redactionProvider;
            _logger = logger;
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
                    FileName = redactPdfRequest.FileName
                };

                return await _redactionProvider.Redact(documentStream, caseId, documentId, pdfRedact, correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogMethodError(correlationId, nameof(RedactAsync), ex.Message, ex);
                throw;
            }
        }
    }
}