using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Clients.Contracts;
using Common.Domain.Document;
using Common.Domain.Exceptions;
using Common.Domain.Extensions;
using Common.Logging;
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
            #region Validate-Inputs
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();

            if (payload == null)
                throw new ArgumentException($"{nameof(payload)} cannot be null.");

            _log.LogMethodEntry(payload.CorrelationId, loggingName, string.Empty);

            var results = _validatorWrapper.Validate(payload);
            if (results?.Any() == true)
                throw new BadRequestException(string.Join(Environment.NewLine, results), nameof(CaseDocumentOrchestrationPayload));
            #endregion

            Stream documentStream = null;

            FileType fileType = (FileType)(-1);

            if (payload.CmsDocumentTracker != null)
            {
                _log.LogMethodFlow(payload.CorrelationId, loggingName, $"Retrieving Document from DDEI for documentId: '{payload.CmsDocumentTracker.CmsDocumentId}'");

                documentStream = await _ddeiClient.GetDocumentFromFileStoreAsync
                    (
                        payload.CmsDocumentTracker.Path,
                        payload.CmsAuthValues,
                        payload.CorrelationId
                    );

                string fileExtension = payload.CmsDocumentTracker.FileExtension.Replace(".", string.Empty).ToUpperInvariant();
                fileType = Enum.Parse<FileType>(fileExtension);
            }
            else if (payload.PcdRequestTracker != null)
            {
                _log.LogMethodFlow(payload.CorrelationId, loggingName, $"Converting PCD request to HTML for documentId: '{payload.PcdRequestTracker.CmsDocumentId}'");

                documentStream = await _convertPcdRequestToHtmlService.ConvertAsync(payload.PcdRequestTracker.PcdRequest);
                fileType = FileType.HTML;
            }
            else if (payload.DefendantAndChargesTracker != null)
            {
                _log.LogMethodFlow(payload.CorrelationId, loggingName, $"Converting Defendant and Charges to HTML for documentId: '{payload.DefendantAndChargesTracker.CmsDocumentId}'");

                documentStream = await _convertPcdRequestToHtmlService.ConvertAsync(payload.DefendantAndChargesTracker.DefendantsAndCharges);
                fileType = FileType.HTML;
            }

            _log.LogMethodFlow(payload.CorrelationId, loggingName,
                $"Converting document of type: '{fileType}'. Original file: '{payload.CmsCaseUrn}', to PDF fileName: '{payload.BlobName}'");

            Stream pdfStream = null;

            try
            {
                pdfStream = await _pdfGeneratorClient.ConvertToPdfAsync(
                    payload.CorrelationId,
                    payload.CmsAuthValues,
                    payload.CmsCaseId.ToString(),
                    payload.CmsDocumentId,
                    payload.CmsVersionId.ToString(),
                    documentStream,
                    fileType);

                _log.LogMethodFlow(payload.CorrelationId, loggingName, $"Document converted to PDF successfully, beginning upload of '{payload.BlobName}'...");
                await _blobStorageService.UploadDocumentAsync
                    (
                        pdfStream,
                        payload.BlobName,
                        payload.CmsCaseId.ToString(),
                        payload.PolarisDocumentId,
                        payload.CmsVersionId.ToString(),
                        payload.CorrelationId
                    );

                _log.LogMethodFlow(payload.CorrelationId, loggingName, $"'{payload.BlobName}' uploaded successfully");
            }
            finally
            {
                documentStream?.Dispose();
                pdfStream?.Dispose();
            }
        }
    }
}