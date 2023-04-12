using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.Exceptions;
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
                request.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
                if (correlationIdValues == null)
                    throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(request));

                var correlationId = correlationIdValues.First();
                if (!Guid.TryParse(correlationId, out currentCorrelationId) || currentCorrelationId == Guid.Empty)
                    throw new BadRequestException("Invalid correlationId. A valid GUID is required.", correlationId);

                _log.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                request.Headers.TryGetValues(HttpHeaderKeys.CmsAuthValues, out var cmsAuthValuesValues);
                if (cmsAuthValuesValues == null)
                    throw new BadRequestException("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.", nameof(request));
                var cmsAuthValues = cmsAuthValuesValues.First();
                if (string.IsNullOrWhiteSpace(cmsAuthValues))
                    throw new BadRequestException("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.", nameof(request));

                request.Headers.TryGetValues(HttpHeaderKeys.Filetype, out var filetypes);
                if (filetypes == null)
                    throw new BadRequestException("Missing Filetype", nameof(request));
                var filetype = filetypes.First();
                if (string.IsNullOrEmpty(filetype))
                    throw new BadRequestException("Invalid Filetype", correlationId);

                request.Headers.TryGetValues(HttpHeaderKeys.Filetype, out var documentIds);
                if (documentIds == null)
                    throw new BadRequestException("Missing DocumentIds", nameof(request));
                var documentId = documentIds.First();
                if (string.IsNullOrEmpty(filetype))
                    throw new BadRequestException("Invalid DocumentId", correlationId);

                var inputStream = await request.Content.ReadAsStreamAsync();
                var pdfStream = _pdfOrchestratorService.ReadToPdfStream(inputStream, Domain.FileType.DOCX, documentId, currentCorrelationId);
                pdfStream.Position = 0;
                return new FileStreamResult(pdfStream, "application/pdf")
                {
                    FileDownloadName = $"{nameof(ConvertToPdf)}.pdf",
                };
            }
            catch (Exception exception)
            {
                return new ObjectResult(exception.ToString())
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
            finally
            {
                _log.LogMethodExit(currentCorrelationId, loggingName, nameof(ConvertToPdf));
            }
        }
    }
}