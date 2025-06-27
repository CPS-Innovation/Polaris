using Common.Clients.PdfGenerator;
using Common.Services.BlobStorage;
using Common.Services.RenderHtmlService;
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
    public class GeneratePdfFromDefendantsAndCharges : BaseGeneratePdf
    {
        private readonly IConvertModelToHtmlService _convertPcdRequestToHtmlService;
        public GeneratePdfFromDefendantsAndCharges(
            IPdfGeneratorClient pdfGeneratorClient,
            IMdsClient mdsClient,
            Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
            IDdeiArgFactory ddeiArgFactory,
            IConvertModelToHtmlService convertPcdRequestToHtmlService,
            IConfiguration configuration)
            : base(ddeiArgFactory, blobStorageServiceFactory, pdfGeneratorClient, configuration, mdsClient)
        {
            _convertPcdRequestToHtmlService = convertPcdRequestToHtmlService;
        }

        [Function(nameof(GeneratePdfFromDefendantsAndCharges))]
        public new async Task<PdfConversionResponse> Run([ActivityTrigger] DocumentPayload payload)
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
            
            var defendantsAndCharges = await MdsClient.GetDefendantAndChargesAsync(arg);

            return await _convertPcdRequestToHtmlService.ConvertAsync(defendantsAndCharges);
        }
    }
}