using Common.Clients.PdfGenerator;
using Common.Services.BlobStorage;
using coordinator.Domain;
using coordinator.Durable.Activity.GeneratePdf;
using coordinator.Durable.Payloads;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace coordinator.Durable.Activity
{
    public class GeneratePdfFromDocument : BaseGeneratePdf
    {
        public GeneratePdfFromDocument(
            IPdfGeneratorClient pdfGeneratorCLient,
            IDdeiClient ddeiClient,
            IDdeiArgFactory ddeiArgFactory,
            Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
            IConfiguration configuration)
            : base(ddeiClient, ddeiArgFactory, blobStorageServiceFactory, pdfGeneratorCLient, configuration) { }

        [Function(nameof(GeneratePdfFromDocument))]
        public new async Task<PdfConversionResponse> Run([ActivityTrigger] DocumentPayload payload)
        {
            return await base.Run(payload);
        }

        protected override async Task<Stream> GetDocumentStreamAsync(DocumentPayload payload)
        {
            var arg = DdeiArgFactory.CreateDocumentVersionArgDto(
                payload.CmsAuthValues,
                payload.CorrelationId,
                payload.Urn,
                payload.CaseId,
                payload.DocumentId,
                payload.VersionId);

            var result = await DdeiClient.GetDocumentAsync(arg);

            return result.Stream;
        }
    }
}