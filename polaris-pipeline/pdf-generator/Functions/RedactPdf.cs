using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Domain.Extensions;
using Common.Domain.Requests;
using Common.Domain.Responses;
using Common.Exceptions.Contracts;
using Common.Handlers;
using Common.Logging;
using Common.Wrappers;
using FluentValidation;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using pdf_generator.Services.DocumentRedactionService;

namespace pdf_generator.Functions
{
    public class RedactPdf
    {
        private readonly IAuthorizationValidator _authorizationValidator;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IDocumentRedactionService _documentRedactionService;
        private readonly ILogger<RedactPdf> _logger;
        private readonly IValidator<RedactPdfRequest> _requestValidator;

        public RedactPdf(IAuthorizationValidator authorizationValidator, IExceptionHandler exceptionHandler, IJsonConvertWrapper jsonConvertWrapper, IDocumentRedactionService documentRedactionService, 
            ILogger<RedactPdf> logger, IValidator<RedactPdfRequest> requestValidator)
        {
            _authorizationValidator = authorizationValidator;
            _exceptionHandler = exceptionHandler;
            _jsonConvertWrapper = jsonConvertWrapper;
            _documentRedactionService = documentRedactionService;
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
            _logger = logger ;
        }

        [FunctionName("redact-pdf")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "redactPdf")]
            HttpRequestMessage request)
        {
            Guid currentCorrelationId = default;
            const string loggingName = "RedactPdf - Run";
            RedactPdfResponse redactPdfResponse = null;

            try
            {
                request.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
                if (correlationIdValues == null)
                    throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(request));

                var correlationId = correlationIdValues.First();
                if (!Guid.TryParse(correlationId, out currentCorrelationId) || currentCorrelationId == Guid.Empty)
                        throw new BadRequestException("Invalid correlationId. A valid GUID is required.", correlationId);
                
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                var authValidation =
                    await _authorizationValidator.ValidateTokenAsync(request.Headers.Authorization, currentCorrelationId, PipelineScopes.RedactPdf, PipelineRoles.EmptyRole);
                if (!authValidation.Item1)
                    throw new UnauthorizedException("Token validation failed");

                if (request.Content == null)
                    throw new BadRequestException("Request body has no content", nameof(request));

                var content = await request.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new BadRequestException("Request body cannot be null.", nameof(request));
                }

                var redactions = _jsonConvertWrapper.DeserializeObject<RedactPdfRequest>(content);
                var validationResult = await _requestValidator.ValidateAsync(redactions);
                if (!validationResult.IsValid)
                    throw new BadRequestException(validationResult.FlattenErrors(), nameof(request));
                
                //TODO exchange access token via on behalf of?
                //var accessToken = values.First().Replace("Bearer ", "");
                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Beginning to apply redactions for documentId: '{redactions.DocumentId}'");
                redactPdfResponse = await _documentRedactionService.RedactPdfAsync(redactions, "onBehalfOfAccessToken", currentCorrelationId);
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
