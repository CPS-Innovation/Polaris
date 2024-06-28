using Aspose.Pdf;
using Common.Dto.Request;
using Common.Streaming;
using Common.Telemetry;

namespace pdf_redactor.Services.DocumentManipulation.Aspose
{
    public class AsposeDocumentManipulationProvider : IDocumentManipulationProvider
    {
        private readonly ITelemetryClient _telemetryClient;

        public AsposeDocumentManipulationProvider(ITelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        public async Task<Stream> RemovePages(Stream stream, string caseId, string documentId, RemoveDocumentPagesDto removeDocumentPages, Guid correlationId)
        {
            DocumentPagesRemovedEvent telemetryEvent = default;
            try
            {
                var inputStream = await stream.EnsureSeekableAsync();

                telemetryEvent = new DocumentPagesRemovedEvent(
                    correlationId: correlationId,
                    caseId: caseId,
                    documentId: documentId,
                    pageNumbersRemoved: removeDocumentPages.PagesIndexesToRemove,
                    startTime: DateTime.UtcNow,
                    originalBytes: inputStream.Length
                );

                var document = new Document(inputStream);

                telemetryEvent.PdfFormat = document.PdfFormat.ToString();
                telemetryEvent.PageCount = document.Pages.Count;

                document.Pages.Delete(removeDocumentPages.PagesIndexesToRemove);

                var outputStream = new MemoryStream();
                await document.SaveAsync(outputStream, CancellationToken.None);
                outputStream.Position = 0;
                document.Dispose();

                telemetryEvent.Bytes = outputStream.Length;
                telemetryEvent.EndTime = DateTime.UtcNow;
                _telemetryClient.TrackEvent(telemetryEvent);

                return outputStream;

            }
            catch (Exception)
            {
                _telemetryClient.TrackEventFailure(telemetryEvent);
                throw;
            }
        }
    }

}