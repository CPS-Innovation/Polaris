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

        protected async Task<PdfConversionStatus> Run(IDurableActivityContext context)
        {
            var payload = context.GetInput<DocumentPayload>();
            if (payload.FileType == null)
            {
                return PdfConversionStatus.DocumentTypeUnsupported;
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
                return response.Status;
            }

            await _polarisBlobStorageService.UploadBlobAsync(response.PdfStream, new BlobIdType(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Pdf));

            response.PdfStream.Dispose();
            return response.Status;
        }

        protected abstract Task<Stream> GetDocumentStreamAsync(DocumentPayload payload);
    }
}