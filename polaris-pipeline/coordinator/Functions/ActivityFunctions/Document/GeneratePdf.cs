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
using Common.Wrappers.Contracts;
using coordinator.Domain;
using DdeiClient.Services.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace coordinator.Functions.ActivityFunctions.Document
{
    public class GeneratePdf
    {
        private readonly HttpClient _pdfGeneratorHttpClient;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidatorWrapper<CaseDocumentOrchestrationPayload> _validatorWrapper;
        private readonly IDdeiClient _documentExtractionService;
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly ILogger<GeneratePdf> _log;

        const string loggingName = nameof(GeneratePdf);

        public GeneratePdf(
             IHttpClientFactory httpClientFactory,
             IJsonConvertWrapper jsonConvertWrapper,
             IValidatorWrapper<CaseDocumentOrchestrationPayload> validatorWrapper,
             IDdeiClient documentExtractionService,
             IPolarisBlobStorageService blobStorageService,
             IExceptionHandler exceptionHandler,
             ILogger<GeneratePdf> logger)
        {
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

            var blobName = $"{payload.CmsCaseId}/pdfs/{Path.GetFileNameWithoutExtension(payload.CmsFileName)}.pdf";

            _log.LogMethodFlow(payload.CorrelationId, loggingName, $"Retrieving Document from DDEI for documentId: '{payload.CmsDocumentId}'");

            var documentStream = await _documentExtractionService.GetDocumentAsync
                (
                    payload.CmsCaseUrn,
                    payload.CmsCaseId.ToString(),
                    payload.CmsDocumentCategory,
                    payload.CmsDocumentId,
                    payload.CmsAuthValues,
                    payload.CorrelationId
                ) ;

            var fileType = Path.GetExtension(payload.CmsFileName).ToFileType();
            _log.LogMethodFlow(payload.CorrelationId, loggingName,
                $"Converting document of type: '{fileType}'. Original file: '{payload.CmsCaseUrn}', to PDF fileName: '{blobName}'");
            Stream pdfStream = await ConvertToPdf(payload.CorrelationId, payload.CmsAuthValues, payload.CmsDocumentId, documentStream, fileType);

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