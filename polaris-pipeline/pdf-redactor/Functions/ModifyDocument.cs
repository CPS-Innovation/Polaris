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
    public class ModifyDocument
    {
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IDocumentManipulationService _documentManipulationService;
        private readonly ILogger<ModifyDocument> _logger;
        private readonly IValidator<ModifyDocumentWithDocumentDto> _requestValidator;

        public ModifyDocument(
            IExceptionHandler exceptionHandler,
            IJsonConvertWrapper jsonConvertWrapper,
            IDocumentManipulationService documentManipulationService,
            ILogger<ModifyDocument> logger,
            IValidator<ModifyDocumentWithDocumentDto> requestValidator)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _documentManipulationService = documentManipulationService ?? throw new ArgumentNullException(nameof(documentManipulationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
        }

        [Function(nameof(ModifyDocument))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ModifyDocument)] HttpRequest request, string caseUrn, int caseId, string documentId)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = request.Headers.GetCorrelationId();

                request.EnableBuffering();

                if (request.ContentLength == null || !request.Body.CanSeek)
                {
                    throw new BadRequestException("Request body has no content", nameof(request));
                }

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

                var modifications = _jsonConvertWrapper.DeserializeObject<ModifyDocumentWithDocumentDto>(content);

                var validationResult = await _requestValidator.ValidateAsync(modifications);
                if (!validationResult.IsValid)
                {
                    throw new BadRequestException(validationResult.FlattenErrors(), nameof(request));
                }

                var modifiedPdfStream = await _documentManipulationService.RemoveOrRotatePagesAsync(caseId, documentId, modifications, currentCorrelationId);

                return new FileStreamResult(modifiedPdfStream, ContentType.Pdf);
            }
            catch (Exception ex)
            {
                return _exceptionHandler.HandleExceptionNew(ex, currentCorrelationId, nameof(ModifyDocument), _logger);
            }
        }
    }
}