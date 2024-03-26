using System;
using System.IO;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Exceptions;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Extensions;
using Common.Handlers;
using Common.Telemetry;
using Common.Wrappers;
using FluentValidation;
using Microsoft.Extensions.Logging;
using pdf_redactor.Services.DocumentRedaction;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Common.Streaming;

namespace pdf_redactor.Functions
{
    public class PdfRedactor
    {
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IDocumentRedactionService _documentRedactionService;
        private readonly ILogger<PdfRedactor> _logger;
        private readonly IValidator<RedactPdfRequestWithDocumentDto> _requestValidator;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;
        private const string PdfContentType = "application/pdf";

        public PdfRedactor(
            IExceptionHandler exceptionHandler,
            IJsonConvertWrapper jsonConvertWrapper,
            IDocumentRedactionService documentRedactionService,
            ILogger<PdfRedactor> logger,
            IValidator<RedactPdfRequestWithDocumentDto> requestValidator,
            ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        {
            _exceptionHandler = exceptionHandler;
            _jsonConvertWrapper = jsonConvertWrapper;
            _documentRedactionService = documentRedactionService;
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper;
            _logger = logger;
        }

        [Function(nameof(PdfRedactor))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.PdfRedactor)] HttpRequest request, string caseUrn, string caseId, string documentId)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = request.Headers.GetCorrelationId();
                _telemetryAugmentationWrapper.RegisterCorrelationId(currentCorrelationId);

                request.EnableBuffering();

                if (request.ContentLength == null || !request.Body.CanSeek)
                    throw new BadRequestException("Request body has no content", nameof(request));

                request.Body.Seek(0, SeekOrigin.Begin);
                string content;
                using (var stream = new StreamReader(request.Body))
                {
                    content = await stream.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new BadRequestException("Request body cannot be null or an empty JSON message", nameof(request));
                }

                var redactions = _jsonConvertWrapper.DeserializeObject<RedactPdfRequestWithDocumentDto>(content);
                _telemetryAugmentationWrapper.RegisterDocumentId(documentId);
                _telemetryAugmentationWrapper.RegisterDocumentVersionId(redactions.VersionId.ToString());

                var validationResult = await _requestValidator.ValidateAsync(redactions);
                if (!validationResult.IsValid)
                    throw new BadRequestException(validationResult.FlattenErrors(), nameof(request));

                var redactPdfStream = await _documentRedactionService.RedactAsync(caseId, documentId, redactions, currentCorrelationId);

                return new FileStreamResult(redactPdfStream, PdfContentType);
            }
            catch (Exception ex)
            {
                return _exceptionHandler.HandleExceptionNew(ex, currentCorrelationId, nameof(RedactPdf), _logger);
            }
        }
    }
}
