using System.IO;
using System.Threading.Tasks;
using Common.Services.BlobStorage;
using Ddei;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using coordinator.Durable.Payloads;
using Common.Clients.PdfGenerator;
using Common.Constants;
using Ddei.Factories;

namespace coordinator.Durable.Activity
{
    public abstract class BaseGeneratePdf
    {
        protected readonly IDdeiClient DdeiClient;
        protected readonly IDdeiArgFactory DdeiArgFactory;
        private readonly IPdfGeneratorClient _pdfGeneratorClient;
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;

        public BaseGeneratePdf(
            IDdeiClient ddeiClient,
            IDdeiArgFactory ddeiArgFactory,
            IPolarisBlobStorageService polarisBlobStorageService,
            IPdfGeneratorClient pdfGeneratorClient)
        {
            DdeiClient = ddeiClient;
            DdeiArgFactory = ddeiArgFactory;
            _pdfGeneratorClient = pdfGeneratorClient;
            _polarisBlobStorageService = polarisBlobStorageService;
        }

        protected async Task<(bool, PdfConversionStatus)> Run(IDurableActivityContext context)
        {
            var payload = context.GetInput<DocumentPayload>();
            var blobId = new BlobIdType(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Pdf);

            if (await _polarisBlobStorageService.BlobExistsAsync(blobId, payload.IsOcredProcessedPreference))
            {
                return (true, 0);
            }

            if (payload.FileType == null)
            {
                return (false, PdfConversionStatus.DocumentTypeUnsupported);
            }

            using var documentStream = await GetDocumentStreamAsync(payload);

            var response = await _pdfGeneratorClient.ConvertToPdfAsync(
                        payload.CorrelationId,
                        payload.Urn,
                        payload.CaseId,
                        payload.DocumentId,
                        payload.VersionId,
                        documentStream,
                        payload.FileType.Value);

            if (response.Status != PdfConversionStatus.DocumentConverted)
            {
                return (false, response.Status);
            }

            await _polarisBlobStorageService.UploadBlobAsync(response.PdfStream, blobId, payload.IsOcredProcessedPreference);
            response.PdfStream.Dispose();

            return (false, PdfConversionStatus.DocumentConverted);
        }

        protected abstract Task<Stream> GetDocumentStreamAsync(DocumentPayload payload);
    }
}