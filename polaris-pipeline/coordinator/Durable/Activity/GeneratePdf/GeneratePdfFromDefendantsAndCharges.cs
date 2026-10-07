using Common.Clients.PdfGenerator;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Services.BlobStorage;
using Common.Services.RenderHtmlService;
using coordinator.Domain;
using coordinator.Durable.Activity.GeneratePdf;
using coordinator.Durable.Payloads;
using Ddei.Factories;
using Ddei.Mappers;
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
        private readonly IMasterDataServiceClient _masterDataServiceClient;
        private readonly ICaseDetailsMapper _caseDetailsMapper;
        private readonly IConvertModelToHtmlService _convertPcdRequestToHtmlService;
        public GeneratePdfFromDefendantsAndCharges(
            IPdfGeneratorClient pdfGeneratorClient,
            IMdsClient mdsClient,
            IMasterDataServiceClient masterDataServiceClient,
            ICaseDetailsMapper caseDetailsMapper,
            Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
            IMdsArgFactory mdsArgFactory,
            IConvertModelToHtmlService convertPcdRequestToHtmlService,
            IConfiguration configuration)
            : base(mdsArgFactory, blobStorageServiceFactory, pdfGeneratorClient, configuration, mdsClient)
        {
            _masterDataServiceClient = masterDataServiceClient;
            _caseDetailsMapper = caseDetailsMapper;
            _convertPcdRequestToHtmlService = convertPcdRequestToHtmlService;
        }

        [Function(nameof(GeneratePdfFromDefendantsAndCharges))]
        public new async Task<PdfConversionResponse> Run([ActivityTrigger] DocumentPayload payload)
        {
            return await base.Run(payload);
        }

        protected override async Task<Stream> GetDocumentStreamAsync(DocumentPayload payload)
        {
            var arg = MdsArgFactory.CreateCaseIdentifiersArg(
                            payload.CmsAuthValues,
                            payload.CorrelationId,
                            payload.Urn,
                            payload.CaseId);

            var defendantsAndCharges = await _masterDataServiceClient.GetCaseDefendantsAsync(
                new ListCaseDefendantsRequest(arg.CaseId, arg.CorrelationId),
                new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));

            var mappedDefendants = _caseDetailsMapper.MapDefendantsResponseToDefendantsAndChargesListDto(defendantsAndCharges, arg.CaseId);

            return await _convertPcdRequestToHtmlService.ConvertAsync(mappedDefendants);
        }
    }
}
