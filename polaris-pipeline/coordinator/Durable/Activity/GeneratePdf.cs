using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Domain.Document;
using Common.Exceptions;
using Common.Services.BlobStorageService;
using coordinator.Services.RenderHtmlService;
using Common.Wrappers;
using DdeiClient.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using coordinator.Durable.Payloads;
using coordinator.Clients.PdfGenerator;
using Common.Constants;

namespace coordinator.Durable.Activity
{
    public class GeneratePdf
    {
        private readonly IConvertModelToHtmlService _convertPcdRequestToHtmlService;
        private readonly IPdfGeneratorClient _pdfGeneratorClient;
        private readonly IValidatorWrapper<CaseDocumentOrchestrationPayload> _validatorWrapper;
        private readonly IDdeiClient _ddeiClient;
        private readonly IPolarisBlobStorageService _blobStorageService;

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
        }

        // todo: for the time being we have a boolean success flag return value. The coordinator refactor 
        //  exercise will do something better than this.
        [FunctionName(nameof(GeneratePdf))]
        public async Task<PdfConversionStatus> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();

            FileType fileType;
            var isSupportedFileType = TryGetFileType(payload, out fileType);
            if (!isSupportedFileType)
            {
                return PdfConversionStatus.DocumentTypeUnsupported;
            }

            using var documentStream = await GetDocumentStreamAsync(payload);

            var response = await _pdfGeneratorClient.ConvertToPdfAsync(
                        payload.CorrelationId,
                        payload.CmsCaseUrn,
                        payload.CmsCaseId.ToString(),
                        payload.CmsDocumentId,
                        payload.CmsVersionId.ToString(),
                        documentStream,
                        fileType);

            if (response.Status != PdfConversionStatus.DocumentConverted)
            {
                return response.Status;
            }

            await _blobStorageService.UploadDocumentAsync
            (
                response.PdfStream,
                payload.BlobName,
                payload.CmsCaseId.ToString(),
                payload.PolarisDocumentId,
                payload.CmsVersionId.ToString(),
                payload.CorrelationId
            );

            response.PdfStream.Dispose();
            return response.Status;
        }

        private bool TryGetFileType(CaseDocumentOrchestrationPayload payload, out FileType fileType)
        {
            var isAPseudoDocument = payload.PcdRequestTracker != null || payload.DefendantAndChargesTracker != null;
            if (isAPseudoDocument)
            {
                fileType = FileType.HTML;
                return true;
            }

            var fileExtension = payload.CmsDocumentTracker.CmsOriginalFileExtension
                .Replace(".", string.Empty)
                .ToUpperInvariant();

            return Enum.TryParse(fileExtension, out fileType);

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