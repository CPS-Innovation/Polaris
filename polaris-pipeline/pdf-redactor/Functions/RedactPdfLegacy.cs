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
using Common.Wrappers;
using FluentValidation;
using pdf_redactor.Services.DocumentRedaction;

namespace pdf_redactor.Functions
{
    public class RedactPdfLegacy
    {
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IDocumentRedactionService _documentRedactionService;
        private readonly ILogger<RedactPdfLegacy> _logger;
        private readonly IValidator<RedactPdfRequestWithDocumentDto> _requestValidator;

        public RedactPdfLegacy(
            IExceptionHandler exceptionHandler,
            IJsonConvertWrapper jsonConvertWrapper,
            IDocumentRedactionService documentRedactionService,
            ILogger<RedactPdfLegacy> logger,
            IValidator<RedactPdfRequestWithDocumentDto> requestValidator)
        {
            _exceptionHandler = exceptionHandler;
            _jsonConvertWrapper = jsonConvertWrapper;
            _documentRedactionService = documentRedactionService;
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
            _logger = logger;
        }

        [Function(nameof(RedactPdfLegacy))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RedactDocumentLegacy)] HttpRequest request,
            string caseUrn,
            int caseId,
            string materialId,
            long documentId)
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

                var redactions = this._jsonConvertWrapper.DeserializeObject<RedactPdfRequestWithDocumentDto>(content);

                var validationResult = await this._requestValidator.ValidateAsync(redactions);
                if (!validationResult.IsValid)
                {
                    throw new BadRequestException(validationResult.FlattenErrors(), nameof(request));
                }

                var redactPdfStream = await this._documentRedactionService.RedactAsync(caseId, materialId, redactions, currentCorrelationId);

                return new FileStreamResult(redactPdfStream, ContentType.Pdf);
            }
            catch (Exception ex)
            {
                return this._exceptionHandler.HandleExceptionNew(ex, currentCorrelationId, nameof(RedactPdfLegacy), this._logger);
            }
        }
    }
}
