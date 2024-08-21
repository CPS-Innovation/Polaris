using System;
using System.IO;
using System.Threading.Tasks;
using Aspose.Pdf;
using Aspose.Pdf.Devices;
using Common.Dto.Request;
using Common.Logging;
using Common.Streaming;

using Microsoft.Extensions.Logging;

namespace pdf_generator.Services.ThumbnailGeneration
{
    public class ThumbnailGenerationService : IThumbnailGenerationService
    {
        private readonly ILogger<ThumbnailGenerationService> _logger;

        public ThumbnailGenerationService(
            ILogger<ThumbnailGenerationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Stream> GenerateThumbnail(string caseId, string documentId, GenerateThumbnailWithDocumentDto thumbnailDetails, Guid correlationId)
        {
            try
            {
                byte[] documentBytes = Convert.FromBase64String(thumbnailDetails.Document);
                using var documentStream = new MemoryStream(documentBytes);

                var inputStream = await documentStream.EnsureSeekableAsync();

                var document = new Document(inputStream);

                var page = document.Pages[thumbnailDetails.ThumbnailParams.PageIndex ?? 1];

                var memoryStream = new MemoryStream();
                JpegDevice jpegDevice = new JpegDevice((int)thumbnailDetails.ThumbnailParams.Height, (int)thumbnailDetails.ThumbnailParams.Width);
                jpegDevice.Process(page, memoryStream);

                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogMethodError(correlationId, nameof(GenerateThumbnail), ex.Message, ex);
                throw;
            }
        }
    }
}