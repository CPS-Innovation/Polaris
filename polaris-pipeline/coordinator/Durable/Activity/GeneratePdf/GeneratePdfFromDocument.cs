using System.IO;
using System.Threading.Tasks;
using Common.Services.BlobStorage;
using Ddei;
using coordinator.Durable.Payloads;
using Common.Clients.PdfGenerator;
using Ddei.Factories;
using System;
using coordinator.Durable.Activity.GeneratePdf;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker;
using coordinator.Domain;

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
             => await DdeiClient.GetDocumentFromFileStoreAsync(
                    payload.Path,
                    payload.CmsAuthValues,
                    payload.CorrelationId);
    }
}