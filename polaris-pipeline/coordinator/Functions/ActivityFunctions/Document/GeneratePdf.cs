using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using coordinator.Clients.Contracts;
using Common.Domain.Document;
using Common.Domain.Exceptions;
using Common.Logging;
using Common.Services.BlobStorageService.Contracts;
using coordinator.Services.RenderHtmlService.Contract;
using Common.Wrappers.Contracts;
using coordinator.Domain;
using DdeiClient.Services.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ActivityFunctions.Document
{
    public class GeneratePdf
    {
        private readonly IConvertModelToHtmlService _convertPcdRequestToHtmlService;
        private readonly IPdfGeneratorClient _pdfGeneratorClient;
        private readonly IValidatorWrapper<CaseDocumentOrchestrationPayload> _validatorWrapper;
        private readonly IDdeiClient _ddeiClient;
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly ILogger<GeneratePdf> _log;

        const string loggingName = nameof(GeneratePdf);

        public GeneratePdf(
            IConvertModelToHtmlService convertPcdRequestToHtmlService,
            IPdfGeneratorClient pdfGeneratorCLient,
            IValidatorWrapper<CaseDocumentOrchestrationPayload> validatorWrapper,
            IDdeiClient ddeiClient,
            IPolarisBlobStorageService blobStorageService,
            ILogger<GeneratePdf> logger)
        {
            _convertPcdRequestToHtmlService = convertPcdRequestToHtmlService;
            _pdfGeneratorClient = pdfGeneratorCLient;
            _validatorWrapper = validatorWrapper;
            _ddeiClient = ddeiClient;
            _blobStorageService = blobStorageService;
            _log = logger;
        }

        [FunctionName(nameof(GeneratePdf))]
        public async Task Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();

            if (payload == null)
                throw new ArgumentException($"{nameof(payload)} cannot be null.");

            var results = _validatorWrapper.Validate(payload);
            if (results?.Any() == true)
                throw new BadRequestException(string.Join(Environment.NewLine, results), nameof(CaseDocumentOrchestrationPayload));

            Stream documentStream = null;

            FileType fileType = (FileType)(-1);

            if (payload.CmsDocumentTracker != null)
            {
                documentStream = await _ddeiClient.GetDocumentFromFileStoreAsync
                    (
                        payload.CmsDocumentTracker.Path,
                        payload.CmsAuthValues,
                        payload.CorrelationId
                    );

                string fileExtension = payload.CmsDocumentTracker.CmsOriginalFileExtension.Replace(".", string.Empty).ToUpperInvariant();
                fileType = Enum.Parse<FileType>(fileExtension);
            }
            else if (payload.PcdRequestTracker != null)
            {
                documentStream = await _convertPcdRequestToHtmlService.ConvertAsync(payload.PcdRequestTracker.PcdRequest);
                fileType = FileType.HTML;
            }
            else if (payload.DefendantAndChargesTracker != null)
            {
                documentStream = await _convertPcdRequestToHtmlService.ConvertAsync(payload.DefendantAndChargesTracker.DefendantsAndCharges);
                fileType = FileType.HTML;
            }

            Stream pdfStream = null;

            try
            {
                pdfStream = await _pdfGeneratorClient.ConvertToPdfAsync(
                    payload.CorrelationId,
                    payload.CmsAuthValues,
                    payload.CmsCaseUrn,
                    payload.CmsCaseId.ToString(),
                    payload.CmsDocumentId,
                    payload.CmsVersionId.ToString(),
                    documentStream,
                    fileType);

                await _blobStorageService.UploadDocumentAsync
                    (
                        pdfStream,
                        payload.BlobName,
                        payload.CmsCaseId.ToString(),
                        payload.PolarisDocumentId,
                        payload.CmsVersionId.ToString(),
                        payload.CorrelationId
                    );
            }
            finally
            {
                documentStream?.Dispose();
                pdfStream?.Dispose();
            }
        }
    }
}