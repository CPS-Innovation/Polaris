using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Clients.Contracts;
using Common.Domain.Document;
using Common.Domain.Exceptions;
using Common.Services.BlobStorageService.Contracts;
using Common.Services.RenderHtmlService.Contract;
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
        private readonly ILogger<GeneratePdf> _logger;
        private const string loggingName = nameof(GeneratePdf);

        public GeneratePdf(
            IConvertModelToHtmlService convertPcdRequestToHtmlService,
            IPdfGeneratorClient pdfGeneratorCLient,
            IValidatorWrapper<CaseDocumentOrchestrationPayload> validatorWrapper,
            IDdeiClient ddeiClient,
            IPolarisBlobStorageService blobStorageService,
            ILogger<GeneratePdf> logger)
        {
            _convertPcdRequestToHtmlService = convertPcdRequestToHtmlService ?? throw new ArgumentNullException(nameof(convertPcdRequestToHtmlService));
            _pdfGeneratorClient = pdfGeneratorCLient ?? throw new ArgumentNullException(nameof(pdfGeneratorCLient));
            _validatorWrapper = validatorWrapper ?? throw new ArgumentNullException(nameof(validatorWrapper));
            _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            var fileType = GetFileType(payload);

            using var documentStream = await GetDocumentStreamAsync(payload);
            using var pdfStream = await _pdfGeneratorClient.ConvertToPdfAsync(
                    payload.CorrelationId,
                    payload.CmsAuthValues,
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

        private FileType GetFileType(CaseDocumentOrchestrationPayload payload)
        {
            if (payload.PcdRequestTracker != null || payload.DefendantAndChargesTracker != null)
            {
                return FileType.HTML;
            }
            else
            {
                var fileExtension = payload.CmsDocumentTracker.CmsOriginalFileExtension
                    .Replace(".", string.Empty)
                    .ToUpperInvariant();

                return Enum.Parse<FileType>(fileExtension);
            }
        }

        private async Task<Stream> GetDocumentStreamAsync(CaseDocumentOrchestrationPayload payload)
        {

            if (payload.PcdRequestTracker != null)
            {
                return await _convertPcdRequestToHtmlService.ConvertAsync(payload.PcdRequestTracker.PcdRequest);
            }
            else if (payload.DefendantAndChargesTracker != null)
            {
                return await _convertPcdRequestToHtmlService.ConvertAsync(payload.DefendantAndChargesTracker.DefendantsAndCharges);
            }
            else
            {
                return await _ddeiClient.GetDocumentFromFileStoreAsync(
                        payload.CmsDocumentTracker.Path,
                        payload.CmsAuthValues,
                        payload.CorrelationId);
            }
        }
    }
}