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

        public async Task<Stream> RemovePagesAsync(string caseId, string documentId, RemoveDocumentPagesWithDocumentDto removeDocumentPages, Guid correlationId)
        {
            try
            {
                byte[] documentBytes = Convert.FromBase64String(removeDocumentPages.Document);
                using var documentStream = new MemoryStream(documentBytes);

                var removePages = new RemoveDocumentPagesDto
                {
                    PagesIndexesToRemove = removeDocumentPages.PagesIndexesToRemove
                };

                return await _documentManipulationProvider.RemovePages(documentStream, caseId, documentId, removePages, correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogMethodError(correlationId, nameof(RemovePagesAsync), ex.Message, ex);
                throw;
            }
        }
    }
}