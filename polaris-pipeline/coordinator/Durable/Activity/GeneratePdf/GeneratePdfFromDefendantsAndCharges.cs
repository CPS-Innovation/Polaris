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
using Common.Dto.Response.Case;
using coordinator.Durable.Activity.GeneratePdf;
using Microsoft.Extensions.Configuration;


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

        [FunctionName(nameof(GeneratePdfFromDefendantsAndCharges))]
        public new async Task<(bool, PdfConversionStatus)> Run([ActivityTrigger] IDurableActivityContext context)
        {
            return await base.Run(context);
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