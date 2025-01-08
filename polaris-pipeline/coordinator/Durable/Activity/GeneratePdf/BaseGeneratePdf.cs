using System;
using System.IO;
using System.Threading.Tasks;
using Common.Clients.PdfGenerator;
using Common.Configuration;
using Common.Constants;
using Common.Services.BlobStorage;
using coordinator.Domain;
using coordinator.Durable.Payloads;
using Ddei;
using Ddei.Factories;
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

        protected async Task<PdfConversionResponse> Run(DocumentPayload payload)
        {
            var blobId = new BlobIdType(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Pdf);

            if (await _polarisBlobStorageService.BlobExistsAsync(blobId, payload.IsOcredProcessedPreference))
            {
                return new PdfConversionResponse { BlobAlreadyExists = true, PdfConversionStatus = PdfConversionStatus.DocumentConverted };
            }

            if (payload.FileType == null)
            {
                return new PdfConversionResponse { BlobAlreadyExists = false, PdfConversionStatus = PdfConversionStatus.DocumentTypeUnsupported };
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
                return new PdfConversionResponse { BlobAlreadyExists = false, PdfConversionStatus = response.Status };
            }

            await _polarisBlobStorageService.UploadBlobAsync(response.PdfStream, blobId, payload.IsOcredProcessedPreference);
            await response.PdfStream.DisposeAsync();

            return new PdfConversionResponse { BlobAlreadyExists = false, PdfConversionStatus = PdfConversionStatus.DocumentConverted };
        }

        protected abstract Task<Stream> GetDocumentStreamAsync(DocumentPayload payload);
    }
}