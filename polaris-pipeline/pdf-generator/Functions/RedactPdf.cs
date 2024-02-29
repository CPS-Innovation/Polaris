using System;
using System.IO;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Domain.Exceptions;
using Common.Domain.Extensions;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Extensions;
using Common.Handlers.Contracts;
using Common.Telemetry.Wrappers.Contracts;
using Common.Wrappers.Contracts;
using FluentValidation;
using Microsoft.Extensions.Logging;
using pdf_generator.Services.DocumentRedaction;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace pdf_generator.Functions
{
    public class RedactPdf
    {
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IDocumentRedactionService _documentRedactionService;
        private readonly ILogger<RedactPdf> _logger;
        private readonly IValidator<RedactPdfRequestDto> _requestValidator;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;

        public RedactPdf(
            IExceptionHandler exceptionHandler,
            IJsonConvertWrapper jsonConvertWrapper,
            IDocumentRedactionService documentRedactionService,
            ILogger<RedactPdf> logger,
            IValidator<RedactPdfRequestDto> requestValidator,
            ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        {
            _exceptionHandler = exceptionHandler;
            _jsonConvertWrapper = jsonConvertWrapper;
            _documentRedactionService = documentRedactionService;
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper;
            _logger = logger;
        }

        [Function(nameof(RedactPdf))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = RestApi.RedactPdf)] HttpRequest request, string caseUrn, string caseId, string documentId)
        {
            Guid currentCorrelationId = default;
            const string loggingName = "RedactPdf - Run";
            RedactPdfResponse redactPdfResponse = null;

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

                var redactions = _jsonConvertWrapper.DeserializeObject<RedactPdfRequestDto>(content);
                _telemetryAugmentationWrapper.RegisterDocumentId(documentId);
                _telemetryAugmentationWrapper.RegisterDocumentVersionId(redactions.VersionId.ToString());

                var validationResult = await _requestValidator.ValidateAsync(redactions);
                if (!validationResult.IsValid)
                    throw new BadRequestException(validationResult.FlattenErrors(), nameof(request));

                redactPdfResponse = await _documentRedactionService.RedactPdfAsync(caseId, documentId, redactions, currentCorrelationId);

                return new OkObjectResult(_jsonConvertWrapper.SerializeObject(redactPdfResponse));
            }
            catch (Exception ex)
            {
                return _exceptionHandler.HandleExceptionNew(ex, currentCorrelationId, nameof(RedactPdf), _logger);
            }
        }
    }
}
