using System;
using System.IO;
using System.Threading.Tasks;
using Common.Services.BlobStorage;
using Ddei;
using coordinator.Durable.Payloads;
using Common.Clients.PdfGenerator;
using Common.Constants;
using Ddei.Factories;
using Common.Services.RenderHtmlService;
using coordinator.Durable.Activity.GeneratePdf;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker;


namespace coordinator.Durable.Activity
{
    public class GeneratePdfFromDefendantsAndCharges : BaseGeneratePdf
    {
        private readonly IConvertModelToHtmlService _convertPcdRequestToHtmlService;
        public GeneratePdfFromDefendantsAndCharges(
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

        [Function(nameof(GeneratePdfFromDefendantsAndCharges))]
        public new async Task<(bool, PdfConversionStatus)> Run([ActivityTrigger] DocumentPayload payload)
        {
            return await base.Run(payload);
        }

        protected override async Task<Stream> GetDocumentStreamAsync(DocumentPayload payload)
        {
            var arg = DdeiArgFactory.CreateCaseIdentifiersArg(
                            payload.CmsAuthValues,
                            payload.CorrelationId,
                            payload.Urn,
                            payload.CaseId);

            var defendantsAndCharges = await DdeiClient.GetDefendantAndChargesAsync(arg);

            return await _convertPcdRequestToHtmlService.ConvertAsync(defendantsAndCharges);
        }
    }
}