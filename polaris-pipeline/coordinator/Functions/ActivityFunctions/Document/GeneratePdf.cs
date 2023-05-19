using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.Document;
using Common.Domain.Exceptions;
using Common.Domain.Extensions;
using Common.Dto.Response;
using Common.Handlers.Contracts;
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
        private readonly HttpClient _pdfGeneratorHttpClient;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidatorWrapper<CaseDocumentOrchestrationPayload> _validatorWrapper;
        private readonly IDdeiClient _documentExtractionService;
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly ILogger<GeneratePdf> _log;

        const string loggingName = nameof(GeneratePdf);

        public GeneratePdf(
            IConvertModelToHtmlService convertPcdRequestToHtmlService,
            IHttpClientFactory httpClientFactory,
            IJsonConvertWrapper jsonConvertWrapper,
            IValidatorWrapper<CaseDocumentOrchestrationPayload> validatorWrapper,
            IDdeiClient documentExtractionService,
            IPolarisBlobStorageService blobStorageService,
            IExceptionHandler exceptionHandler,
            ILogger<GeneratePdf> logger)
        {
            _convertPcdRequestToHtmlService = convertPcdRequestToHtmlService;
            _pdfGeneratorHttpClient = httpClientFactory.CreateClient(nameof(GeneratePdf));
            _jsonConvertWrapper = jsonConvertWrapper;
            _validatorWrapper = validatorWrapper;
            _documentExtractionService = documentExtractionService;
            _blobStorageService = blobStorageService;
            _exceptionHandler = exceptionHandler;
            _log = logger;
        }

        [FunctionName(nameof(GeneratePdf))]
        public async Task<GeneratePdfResponse> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();

            if (payload == null)
                throw new ArgumentException($"{nameof(payload)} cannot be null.");

            _log.LogMethodEntry(payload.CorrelationId, loggingName, string.Empty);

            var results = _validatorWrapper.Validate(payload);
            if (results?.Any() == true)
                throw new BadRequestException(string.Join(Environment.NewLine, results), nameof(CaseDocumentOrchestrationPayload));

            Stream documentStream = null;
            string blobName = null;
            FileType fileType = (FileType)(-1);

            if(payload.CmsDocumentTracker != null)
            {
                _log.LogMethodFlow(payload.CorrelationId, loggingName, $"Retrieving Document from DDEI for documentId: '{payload.CmsDocumentTracker.CmsDocumentId}'");

                documentStream = await _documentExtractionService.GetDocumentAsync
                    (
                        payload.CmsCaseUrn,
                        payload.CmsCaseId.ToString(),
                        payload.CmsDocumentTracker.CmsDocType.DocumentCategory,
                        payload.CmsDocumentTracker.CmsDocumentId,
                        payload.CmsAuthValues,
                        payload.CorrelationId
                    );
                blobName = $"{payload.CmsCaseId}/pdfs/CMS-{Path.GetFileNameWithoutExtension(payload.CmsDocumentTracker.CmsDocumentId)}.pdf";
                fileType = Path.GetExtension(payload.CmsDocumentTracker.CmsOriginalFileName).ToFileType();
            }
            else if(payload.PcdRequestTracker != null) 
            {
                _log.LogMethodFlow(payload.CorrelationId, loggingName, $"Converting PCD request to HTML for documentId: '{payload.PcdRequestTracker.CmsDocumentId}'");

                blobName = $"{payload.CmsCaseId}/pdfs/{Path.GetFileNameWithoutExtension(payload.PcdRequestTracker.CmsDocumentId)}.pdf";
                documentStream = await _convertPcdRequestToHtmlService.ConvertAsync(payload.PcdRequestTracker.PcdRequest);
                fileType = FileType.HTML;
            }
            else if (payload.DefendantAndChargesTracker != null)
            {
                _log.LogMethodFlow(payload.CorrelationId, loggingName, $"Converting Defendant and Charges to HTML for documentId: '{payload.DefendantAndChargesTracker.CmsDocumentId}'");

                blobName = $"{payload.CmsCaseId}/pdfs/{Path.GetFileNameWithoutExtension(payload.DefendantAndChargesTracker.CmsDocumentId)}.pdf";
                documentStream = await _convertPcdRequestToHtmlService.ConvertAsync(payload.DefendantAndChargesTracker.DefendantsAndCharges);
                fileType = FileType.HTML;
            }

            _log.LogMethodFlow(payload.CorrelationId, loggingName,
                $"Converting document of type: '{fileType}'. Original file: '{payload.CmsCaseUrn}', to PDF fileName: '{blobName}'");

            Stream pdfStream = null;

            try
            {
                pdfStream = await ConvertToPdf(payload.CorrelationId, payload.CmsAuthValues, payload.CmsDocumentId, documentStream, fileType);

                _log.LogMethodFlow(payload.CorrelationId, loggingName, $"Document converted to PDF successfully, beginning upload of '{blobName}'...");
                await _blobStorageService.UploadDocumentAsync
                    (
                        pdfStream,
                        blobName,
                        payload.CmsCaseId.ToString(),
                        payload.CmsDocumentId,
                        payload.CmsVersionId.ToString(),
                        payload.CorrelationId
                    );

                _log.LogMethodFlow(payload.CorrelationId, loggingName, $"'{blobName}' uploaded successfully");
            }
            finally 
            {
                documentStream?.Dispose();
                pdfStream?.Dispose();
            }

            var generatePdfResponse = new GeneratePdfResponse(blobName);
            return generatePdfResponse;
        }

        private async Task<Stream> ConvertToPdf(Guid correlationId, string cmsAuthValues, string documentId, Stream documentStream, FileType fileType)
        {
            var pdfStream = new MemoryStream();

            documentStream.Seek(0, SeekOrigin.Begin);
            var request = new HttpRequestMessage(HttpMethod.Post, "convert-to-pdf");
            using (var requestContent = new StreamContent(documentStream))
            {
                request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
                request.Headers.Add(HttpHeaderKeys.CmsAuthValues, cmsAuthValues);
                request.Headers.Add(HttpHeaderKeys.DocumentId, documentId);
                request.Headers.Add(HttpHeaderKeys.Filetype, fileType.ToString());

                request.Content = requestContent;

                using (var response = await _pdfGeneratorHttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    await response.Content.CopyToAsync(pdfStream);
                    pdfStream.Seek(0, SeekOrigin.Begin);
                }
            }

            return pdfStream;
        }
    }
}