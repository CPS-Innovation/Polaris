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
namespace coordinator.Durable.Activity
{
    public class GeneratePdfFromDefendantsAndCharges : BaseGeneratePdf
    {
        private readonly IConvertModelToHtmlService _convertPcdRequestToHtmlService;
        public GeneratePdfFromDefendantsAndCharges(
            IPdfGeneratorClient pdfGeneratorClient,
            IDdeiClient ddeiClient,
            IPolarisBlobStorageService polarisBlobStorageService,
            IDdeiArgFactory ddeiArgFactory,
            IConvertModelToHtmlService convertPcdRequestToHtmlService)
            : base(ddeiClient, ddeiArgFactory, polarisBlobStorageService, pdfGeneratorClient)
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

            var defendantsAndChargesResult = await DdeiClient.GetDefendantAndChargesAsync(arg);
            var defendantsAndCharges = new DefendantsAndChargesListDto
            {
                CaseId = payload.CaseId,
                DefendantsAndCharges = defendantsAndChargesResult
            };
            return await _convertPcdRequestToHtmlService.ConvertAsync(defendantsAndCharges);
        }
    }
}