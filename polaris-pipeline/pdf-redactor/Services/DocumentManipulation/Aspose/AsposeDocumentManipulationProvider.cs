using Aspose.Pdf;
using Common.Constants;
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

        public async Task<Stream> ModifyDocument(Stream stream, string caseId, string documentId, ModifyDocumentDto modifications, Guid correlationId)
        {
            DocumentModifiedEvent telemetryEvent = default;
            try
            {
                var inputStream = await stream.EnsureSeekableAsync();

                var pagesRemoved = modifications.DocumentModifications.Where(x => x.Operation == DocumentManipulationOperation.RemovePage).SelectMany(x => new[] { x.PageIndex }).ToArray();
                var pagesRotated = modifications.DocumentModifications.Where(x => x.Operation == DocumentManipulationOperation.RotatePage).SelectMany(x => new[] { x.PageIndex }).ToArray();

                telemetryEvent = new DocumentModifiedEvent(
                    correlationId: correlationId,
                    caseId: caseId,
                    documentId: documentId,
                    pageNumbersRemoved: pagesRemoved,
                    pageNumbersRotated: pagesRotated,
                    startTime: DateTime.UtcNow,
                    originalBytes: inputStream.Length
                );

                var document = new Document(inputStream);

                telemetryEvent.PdfFormat = document.PdfFormat.ToString();
                telemetryEvent.PageCount = document.Pages.Count;

                foreach (var change in modifications.DocumentModifications.OrderByDescending(x => x.PageIndex))
                {
                    switch (change.Operation)
                    {
                        case DocumentManipulationOperation.RemovePage:
                            document.Pages.Delete(change.PageIndex);
                            break;
                        case DocumentManipulationOperation.RotatePage:
                            document.Pages[change.PageIndex].Rotate = GetRotation(change.Arg.ToString());
                            break;
                    }
                }

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

        private static Rotation GetRotation(string value) => value switch
        {
            "90" => Rotation.on90,
            "180" => Rotation.on180,
            "270" => Rotation.on270,
            _ => throw new Exception("Rotation input value not recognised.")
        };
    }
}