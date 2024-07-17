using Common.Dto.Request;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace pdf_redactor.Services.DocumentManipulation
{
    public class DocumentManipulationService : IDocumentManipulationService
    {
        private readonly IDocumentManipulationProvider _documentManipulationProvider;
        private readonly ILogger<DocumentManipulationService> _logger;

        public DocumentManipulationService(
            IDocumentManipulationProvider documentManipulationProvider,
            ILogger<DocumentManipulationService> logger)
        {
            _documentManipulationProvider = documentManipulationProvider ?? throw new ArgumentNullException(nameof(documentManipulationProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Stream> RemoveOrRotatePagesAsync(string caseId, string documentId, ModifyDocumentWithDocumentDto removeOrRotateDocumentPages, Guid correlationId)
        {
            try
            {
                byte[] documentBytes = Convert.FromBase64String(removeOrRotateDocumentPages.Document);
                using var documentStream = new MemoryStream(documentBytes);

                var modifications = new ModifyDocumentDto
                {
                    DocumentChanges = removeOrRotateDocumentPages.DocumentChanges
                };

                return await _documentManipulationProvider.ModifyDocument(documentStream, caseId, documentId, modifications, correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogMethodError(correlationId, nameof(RemoveOrRotatePagesAsync), ex.Message, ex);
                throw;
            }
        }
    }
}