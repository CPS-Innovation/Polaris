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
using System;
using System.Threading.Tasks;
using System.IO;
using pdf_generator.Services.ThumbnailGeneration;

namespace pdf_generator.Functions
{
    public class GenerateThumbnail
    {
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IThumbnailGenerationService _thumbnailGenerationService;
        private readonly ILogger<GenerateThumbnail> _logger;
        private readonly IValidator<GenerateThumbnailWithDocumentDto> _requestValidator;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;

        public GenerateThumbnail(
            IExceptionHandler exceptionHandler,
            IJsonConvertWrapper jsonConvertWrapper,
            IThumbnailGenerationService thumbnailGenerationService,
            ILogger<GenerateThumbnail> logger,
            IValidator<GenerateThumbnailWithDocumentDto> requestValidator,
            ITelemetryAugmentationWrapper telemetryAugmentationWrapper
        )
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _thumbnailGenerationService = thumbnailGenerationService ?? throw new ArgumentNullException(nameof(thumbnailGenerationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper ?? throw new ArgumentNullException(nameof(telemetryAugmentationWrapper));
        }

        [Function(nameof(GenerateThumbnail))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.GenerateThumbnail)] HttpRequest request, string caseUrn, string caseId, string documentId)
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

                var thumbnailDetails = _jsonConvertWrapper.DeserializeObject<GenerateThumbnailWithDocumentDto>(content);
                _telemetryAugmentationWrapper.RegisterDocumentId(documentId);
                _telemetryAugmentationWrapper.RegisterDocumentVersionId(thumbnailDetails.VersionId.ToString());

                var validationResult = await _requestValidator.ValidateAsync(thumbnailDetails);
                if (!validationResult.IsValid)
                    throw new BadRequestException(validationResult.FlattenErrors(), nameof(request));

                var thumbnail = await _thumbnailGenerationService.GenerateThumbnail(caseId, documentId, thumbnailDetails, currentCorrelationId);
                thumbnail.Position = 0;

                return new FileStreamResult(thumbnail, ContentType.Jpeg);
            }
            catch (Exception ex)
            {
                return _exceptionHandler.HandleExceptionNew(ex, currentCorrelationId, nameof(GenerateThumbnail), _logger);
            }
        }
    }
}