using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using coordinator.Clients.Contracts;
using Common.Domain.Document;
using Common.Domain.Exceptions;
using Common.Services.BlobStorageService.Contracts;
using coordinator.Services.RenderHtmlService.Contract;
using Common.Wrappers.Contracts;
using DdeiClient.Services.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using coordinator.Durable.Payloads;

namespace coordinator.Durable.Activity
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

        // todo: for the time being we have a boolean success flag return value. The coordinator refactor 
        //  exercise will do something better than this.
        [FunctionName(nameof(GeneratePdf))]
        public async Task<bool> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();

            if (payload == null)
                throw new ArgumentException($"{nameof(payload)} cannot be null.");

            var results = _validatorWrapper.Validate(payload);
            if (results?.Any() == true)
                throw new BadRequestException(string.Join(Environment.NewLine, results), nameof(CaseDocumentOrchestrationPayload));

            var fileType = GetFileType(payload);

            var documentStream = await GetDocumentStreamAsync(payload);

            Stream pdfStream = Stream.Null;
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

            }
            catch (UnsupportedMediaTypeException)
            {
                // If pdf-generator has failed on conversion we arrive here.  The failure will have been logged in the
                //  pdf-generator function app so no need to log here, but we let the caller know we've not converted 
                //  via bool return value.
                return false;
            }

            await _blobStorageService.UploadDocumentAsync
                (
                    pdfStream,
                    payload.BlobName,
                    payload.CmsCaseId.ToString(),
                    payload.PolarisDocumentId,
                    payload.CmsVersionId.ToString(),
                    payload.CorrelationId
                );

            return true;
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