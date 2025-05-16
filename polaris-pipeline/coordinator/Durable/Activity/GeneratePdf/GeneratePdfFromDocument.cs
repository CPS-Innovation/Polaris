using Common.Clients.PdfGenerator;
using Common.Services.BlobStorage;
using coordinator.Domain;
using coordinator.Durable.Activity.GeneratePdf;
using coordinator.Durable.Payloads;
using Ddei.Factories;
using DdeiClient.Enums;
using DdeiClient.Factories;
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
            IPdfGeneratorClient pdfGeneratorClient,
            IDdeiArgFactory ddeiArgFactory,
            Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
            IConfiguration configuration,
            IDdeiClientFactory ddeiClientFactory)
            : base(ddeiArgFactory, blobStorageServiceFactory, pdfGeneratorClient, configuration, ddeiClientFactory) { }

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

            var ddeiClient = DdeiClientFactory.Create(payload.CmsAuthValues, DdeiClients.Mds);
            var result = await ddeiClient.GetDocumentAsync(arg);

            return result.Stream;
        }
    }
}