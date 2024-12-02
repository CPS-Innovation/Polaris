using System.IO;
using System.Threading.Tasks;
using Common.Services.BlobStorage;
using Ddei;
using coordinator.Durable.Payloads;
using Common.Clients.PdfGenerator;
using Ddei.Factories;
using Common.Services.RenderHtmlService;
using System;
using coordinator.Durable.Activity.GeneratePdf;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker;
using coordinator.Domain;

namespace coordinator.Durable.Activity
{
    public class GeneratePdfFromPcdRequest : BaseGeneratePdf
    {
        private readonly IConvertModelToHtmlService _convertPcdRequestToHtmlService;
        public GeneratePdfFromPcdRequest(
            IPdfGeneratorClient pdfGeneratorClient,
            IDdeiClient ddeiClient,
            Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
            IDdeiArgFactory ddeiArgFactory,
            IConvertModelToHtmlService convertPcdRequestToHtmlService,
            IConfiguration configuration)
            : base(ddeiClient, ddeiArgFactory, blobStorageServiceFactory, pdfGeneratorClient, configuration)
        {
            _convertPcdRequestToHtmlService = convertPcdRequestToHtmlService;

        }

        [Function(nameof(GeneratePdfFromPcdRequest))]
        public new async Task<PdfConversionResponse> Run([ActivityTrigger] DocumentPayload payload)
        {
            return await base.Run(payload);
        }

        protected override async Task<Stream> GetDocumentStreamAsync(DocumentPayload payload)
        {
            var arg = DdeiArgFactory.CreatePcdArg(
                    payload.CmsAuthValues,
                    payload.CorrelationId,
                    payload.Urn,
                    payload.CaseId,
                    payload.DocumentId);
            var pcdRequest = await DdeiClient.GetPcdRequestAsync(arg);
            return await _convertPcdRequestToHtmlService.ConvertAsync(pcdRequest);
        }
    }
}