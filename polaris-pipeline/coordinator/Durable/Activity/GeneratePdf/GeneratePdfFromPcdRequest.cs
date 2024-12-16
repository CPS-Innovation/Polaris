using System.IO;
using System.Threading.Tasks;
using Common.Services.BlobStorage;
using Ddei;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using coordinator.Durable.Payloads;
using Common.Clients.PdfGenerator;
using Common.Constants;
using Ddei.Factories;
using Common.Services.RenderHtmlService;
using System;
using coordinator.Durable.Activity.GeneratePdf;
using Microsoft.Extensions.Configuration;

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

        [FunctionName(nameof(GeneratePdfFromPcdRequest))]
        public new async Task<(bool, PdfConversionStatus)> Run([ActivityTrigger] IDurableActivityContext context)
        {
            return await base.Run(context);
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