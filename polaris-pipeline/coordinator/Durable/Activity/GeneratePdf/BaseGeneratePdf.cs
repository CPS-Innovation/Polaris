using System;
using System.IO;
using System.Threading.Tasks;
using Common.Clients.PdfGenerator;
using Common.Configuration;
using Common.Constants;
using Common.Services.BlobStorage;
using coordinator.Durable.Payloads;
using Ddei;
using Ddei.Factories;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;

namespace coordinator.Durable.Activity.GeneratePdf
{
    public abstract class BaseGeneratePdf
    {
        protected readonly IDdeiClient DdeiClient;
        protected readonly IDdeiArgFactory DdeiArgFactory;
        private readonly IPdfGeneratorClient _pdfGeneratorClient;
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;

        protected BaseGeneratePdf(
            IDdeiClient ddeiClient,
            IDdeiArgFactory ddeiArgFactory,
            Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
            IPdfGeneratorClient pdfGeneratorClient, 
            IConfiguration configuration)
        {
            DdeiClient = ddeiClient;
            DdeiArgFactory = ddeiArgFactory;
            _pdfGeneratorClient = pdfGeneratorClient;
            _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
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

            await using var documentStream = await GetDocumentStreamAsync(payload);

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
            await response.PdfStream.DisposeAsync();

            return (false, PdfConversionStatus.DocumentConverted);
        }

        protected abstract Task<Stream> GetDocumentStreamAsync(DocumentPayload payload);
    }
}