using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using Common.Constants;
using Common.Dto.Request;
using Common.Exceptions;
using Common.Extensions;
using Common.Handlers;
using Common.Telemetry;
using Common.Wrappers;
using FluentValidation;
using pdf_redactor.Services.DocumentManipulation;

namespace pdf_redactor.Functions
{
    public class RemoveDocumentPages
    {
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IDocumentManipulationService _documentManipulationService;
        private readonly ILogger<RemoveDocumentPages> _logger;
        private readonly IValidator<RemoveDocumentPagesWithDocumentDto> _requestValidator;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;

        public RemoveDocumentPages(
            IExceptionHandler exceptionHandler,
            IJsonConvertWrapper jsonConvertWrapper,
            IDocumentManipulationService documentManipulationService,
            ILogger<RemoveDocumentPages> logger,
            IValidator<RemoveDocumentPagesWithDocumentDto> requestValidator,
            ITelemetryAugmentationWrapper telemetryAugmentationWrapper
        )
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _documentManipulationService = documentManipulationService ?? throw new ArgumentNullException(nameof(documentManipulationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper ?? throw new ArgumentNullException(nameof(telemetryAugmentationWrapper));
        }

        [Function(nameof(RemoveDocumentPages))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.RemoveDocumentPages)] HttpRequest request, string caseUrn, string caseId, string documentId)
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

                var pageIndexes = _jsonConvertWrapper.DeserializeObject<RemoveDocumentPagesWithDocumentDto>(content);
                _telemetryAugmentationWrapper.RegisterDocumentId(documentId);
                _telemetryAugmentationWrapper.RegisterDocumentVersionId(pageIndexes.VersionId.ToString());

                var validationResult = await _requestValidator.ValidateAsync(pageIndexes);
                if (!validationResult.IsValid)
                    throw new BadRequestException(validationResult.FlattenErrors(), nameof(request));

                var modifiedPdfStream = await _documentManipulationService.RemovePagesAsync(caseId, documentId, pageIndexes, currentCorrelationId);

                return new FileStreamResult(modifiedPdfStream, ContentType.Pdf);
            }
            catch (Exception ex)
            {
                return _exceptionHandler.HandleExceptionNew(ex, currentCorrelationId, nameof(RemoveDocumentPages), _logger);
            }
        }
    }
}