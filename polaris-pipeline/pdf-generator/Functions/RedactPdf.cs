using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Domain.Exceptions;
using Common.Domain.Extensions;
using Common.Extensions;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Handlers.Contracts;
using Common.Logging;
using Common.Telemetry.Wrappers.Contracts;
using Common.Wrappers.Contracts;
using FluentValidation;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using pdf_generator.Services.DocumentRedactionService;

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

        [FunctionName(nameof(RedactPdf))]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = RestApi.RedactPdf)] HttpRequestMessage request)
        {
            Guid currentCorrelationId = default;
            const string loggingName = "RedactPdf - Run";
            RedactPdfResponse redactPdfResponse = null;

            try
            {
                #region Validate-Inputs
                currentCorrelationId = request.Headers.GetCorrelationId();
                _telemetryAugmentationWrapper.RegisterCorrelationId(currentCorrelationId);

                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (request.Content == null)
                    throw new BadRequestException("Request body has no content", nameof(request));

                var content = await request.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new BadRequestException("Request body cannot be null.", nameof(request));
                }
                #endregion

                var redactions = _jsonConvertWrapper.DeserializeObject<RedactPdfRequestDto>(content);
                var validationResult = await _requestValidator.ValidateAsync(redactions);
                if (!validationResult.IsValid)
                    throw new BadRequestException(validationResult.FlattenErrors(), nameof(request));

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Beginning to apply redactions for polarisDocumentId: '{redactions.PolarisDocumentId}'");
                redactPdfResponse = await _documentRedactionService.RedactPdfAsync(redactions, currentCorrelationId);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_jsonConvertWrapper.SerializeObject(redactPdfResponse), Encoding.UTF8,
                        MediaTypeNames.Application.Json)
                };
            }
            catch (Exception ex)
            {
                return _exceptionHandler.HandleException(ex, currentCorrelationId, nameof(RedactPdf), _logger);
            }
            finally
            {
                _logger.LogMethodExit(currentCorrelationId, loggingName, redactPdfResponse.ToJson());
            }
        }
    }
}
