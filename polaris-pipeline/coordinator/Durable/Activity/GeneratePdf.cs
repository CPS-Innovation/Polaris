using System;
using System.IO;
using System.Threading.Tasks;
using Common.Domain.Document;
using Common.Services.BlobStorageService;
using coordinator.Services.RenderHtmlService;
using Common.Wrappers;
using DdeiClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using coordinator.Durable.Payloads;
using coordinator.Clients.PdfGenerator;
using Common.Constants;
using Ddei.Factories;
using Common.Dto.Case;
using System.Linq;

namespace coordinator.Durable.Activity
{
    public class GeneratePdf
    {
        private readonly IConvertModelToHtmlService _convertPcdRequestToHtmlService;
        private readonly IPdfGeneratorClient _pdfGeneratorClient;
        private readonly IValidatorWrapper<CaseDocumentOrchestrationPayload> _validatorWrapper;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly IPolarisBlobStorageService _blobStorageService;

        public GeneratePdf(
            IConvertModelToHtmlService convertPcdRequestToHtmlService,
            IPdfGeneratorClient pdfGeneratorCLient,
            IValidatorWrapper<CaseDocumentOrchestrationPayload> validatorWrapper,
            IDdeiClient ddeiClient,
            IPolarisBlobStorageService blobStorageService,
            IDdeiArgFactory ddeiArgFactory,
            ILogger<GeneratePdf> logger)
        {
            _convertPcdRequestToHtmlService = convertPcdRequestToHtmlService;
            _pdfGeneratorClient = pdfGeneratorCLient;
            _validatorWrapper = validatorWrapper;
            _ddeiClient = ddeiClient;
            _blobStorageService = blobStorageService;
            _ddeiArgFactory = ddeiArgFactory;
        }

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
                        payload.Urn,
                        payload.CaseId.ToString(),
                        payload.DocumentId,
                        payload.VersionId.ToString(),
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
                payload.CaseId.ToString(),
                payload.DocumentId,
                payload.VersionId.ToString(),
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

            var fileExtension = Path.GetExtension(payload.CmsDocumentTracker.CmsOriginalFileName)
                .Replace(".", string.Empty)
                .ToUpperInvariant();

            return Enum.TryParse(fileExtension, out fileType);

        }

        private async Task<Stream> GetDocumentStreamAsync(CaseDocumentOrchestrationPayload payload)
        {
            if (payload.PcdRequestTracker != null)
            {
                var arg = _ddeiArgFactory.CreatePcdArg(
                    payload.CmsAuthValues,
                    payload.CorrelationId,
                    payload.Urn,
                    payload.CaseId,
                    payload.PcdRequestTracker.PcdRequest.Id);
                var pcdRequest = await _ddeiClient.GetPcdRequest(arg);
                return await _convertPcdRequestToHtmlService.ConvertAsync(pcdRequest);
            }
            else if (payload.DefendantAndChargesTracker != null)
            {
                var arg = _ddeiArgFactory.CreateCaseArg(
                    payload.CmsAuthValues,
                    payload.CorrelationId,
                    payload.Urn,
                    payload.CaseId);

                var defendantsAndChargesResult = await _ddeiClient.GetDefendantAndCharges(arg);

                var defendantsAndCharges = new DefendantsAndChargesListDto
                {
                    CaseId = payload.CaseId,
                    DefendantsAndCharges = defendantsAndChargesResult.OrderBy(dac => dac.ListOrder)
                };
                return await _convertPcdRequestToHtmlService.ConvertAsync(defendantsAndCharges);
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