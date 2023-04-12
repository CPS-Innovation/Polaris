using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Dto.Request;
using Common.Exceptions.Contracts;
using Common.Logging;
using Common.Services.BlobStorageService.Contracts;
using Common.Wrappers.Contracts;
using DdeiClient.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using pdf_generator.Services.PdfService;

namespace pdf_generator.Functions
{
    public class ConvertToPdf
    {
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidatorWrapper<GeneratePdfRequestDto> _validatorWrapper;
        private readonly IDdeiClient _documentExtractionService;
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IPdfOrchestratorService _pdfOrchestratorService;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly ILogger<ConvertToPdf> _log;

        const string loggingName = nameof(ConvertToPdf);

        public ConvertToPdf(
             IJsonConvertWrapper jsonConvertWrapper,
             IValidatorWrapper<GeneratePdfRequestDto> validatorWrapper,
             IDdeiClient documentExtractionService,
             IPolarisBlobStorageService blobStorageService,
             IPdfOrchestratorService pdfOrchestratorService,
             IExceptionHandler exceptionHandler,
             ILogger<ConvertToPdf> logger)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _validatorWrapper = validatorWrapper;
            _documentExtractionService = documentExtractionService;
            _blobStorageService = blobStorageService;
            _pdfOrchestratorService = pdfOrchestratorService;
            _exceptionHandler = exceptionHandler;
            _log = logger;
        }

        [FunctionName(nameof(ConvertToPdf))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "convert-to-pdf")] HttpRequestMessage request)
        {
            Guid currentCorrelationId = default;

            try
            {
                var fileType = request.Headers.GetValues("Filetype").FirstOrDefault();
                var documentId = request.Headers.GetValues("DocumentId").FirstOrDefault();
                var inputStream = await request.Content.ReadAsStreamAsync();

                var outputStream = new MemoryStream();

                var pdfStream = _pdfOrchestratorService.ReadToPdfStream(inputStream, Domain.FileType.DOCX, documentId, currentCorrelationId);

                pdfStream.Position = 0;
                return new FileStreamResult(pdfStream, "application/pdf")
                {
                    FileDownloadName = "pdf"//,
                    //EntityTag = new EntityTagHeaderValue("123")
                };

                //pdfStream.CopyTo(outputStream);
                //outputStream.Position = 0;
                //return new FileStreamResult(outputStream, "application/pdf")
                //{
                //    FileDownloadName = "pdf"//,
                //    //EntityTag = new EntityTagHeaderValue("123")
                //};

                //request.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
                //if (correlationIdValues == null)
                //    throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(request));

                //var correlationId = correlationIdValues.First();
                //if (!Guid.TryParse(correlationId, out currentCorrelationId) || currentCorrelationId == Guid.Empty)
                //    throw new BadRequestException("Invalid correlationId. A valid GUID is required.", correlationId);

                //request.Headers.TryGetValues(HttpHeaderKeys.CmsAuthValues, out var cmsAuthValuesValues);
                //if (cmsAuthValuesValues == null)
                //    throw new BadRequestException("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.", nameof(request));
                //var cmsAuthValues = cmsAuthValuesValues.First();
                //if (string.IsNullOrWhiteSpace(cmsAuthValues))
                //    throw new BadRequestException("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.", nameof(request));

                //_log.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                //if (request.Content == null)
                //    throw new BadRequestException("Request body has no content", nameof(request));

                //var content = await request.Content.ReadAsStringAsync();
                //if (string.IsNullOrWhiteSpace(content))
                //    throw new BadRequestException("Request body cannot be null.", nameof(request));

                //var pdfRequest = _jsonConvertWrapper.DeserializeObject<GeneratePdfRequestDto>(content);
                //if (pdfRequest == null)
                //    throw new BadRequestException($"An invalid message was received '{content}'", nameof(request));

                //var results = _validatorWrapper.Validate(pdfRequest);
                //if (results.Any())
                //    throw new BadRequestException(string.Join(Environment.NewLine, results), nameof(request));

                //var blobName = $"{pdfRequest.CaseId}/pdfs/{Path.GetFileNameWithoutExtension(pdfRequest.FileName)}.pdf";
                //generatePdfResponse = new GeneratePdfResponse(blobName);

                //_log.LogMethodFlow(currentCorrelationId, loggingName, $"Retrieving Document from DDEI for documentId: '{pdfRequest.DocumentId}'");

                //// If CMS Document.... TODO If PcdRequest

                //// Step 1 - Get DDEI Document - TODO - move to parent
                //var documentStream = await _documentExtractionService.GetDocumentAsync(pdfRequest.CaseUrn, pdfRequest.CaseId.ToString(), pdfRequest.DocumentCategory,
                //    pdfRequest.DocumentId, cmsAuthValues, currentCorrelationId);

                //var fileType = Path.GetExtension(pdfRequest.FileName).ToFileType();
                //_log.LogMethodFlow(currentCorrelationId, loggingName,
                //    $"Processing retrieved document of type: '{fileType}'. Original file: '{pdfRequest.FileName}', with new fileName: '{blobName}'");

                //// Step 2 - Generate PDF Stream
                //var pdfStream = _pdfOrchestratorService.ReadToPdfStream(documentStream, fileType, pdfRequest.DocumentId, currentCorrelationId);

                //// Step 3 - Upload to Blob Storage - TODO - move to parent
                //_log.LogMethodFlow(currentCorrelationId, loggingName, $"Document converted to PDF successfully, beginning upload of '{blobName}'...");
                //await _blobStorageService.UploadDocumentAsync(pdfStream, blobName, pdfRequest.CaseId.ToString(), pdfRequest.DocumentId,
                //    pdfRequest.VersionId.ToString(), currentCorrelationId);

                //_log.LogMethodFlow(currentCorrelationId, loggingName, $"'{blobName}' uploaded successfully");

                //return OkResponse(Serialize(generatePdfResponse));
            }
            catch (Exception exception)
            {
                return new ObjectResult(exception.ToString())
                {
                    StatusCode = 500
                };
            }
            finally
            {
                _log.LogMethodExit(currentCorrelationId, loggingName, nameof(ConvertToPdf));
            }
        }
    }
}