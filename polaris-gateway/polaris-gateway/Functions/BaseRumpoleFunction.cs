using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using PolarisGateway.Domain.Exceptions;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Domain.Validation;
using PolarisGateway.Domain.Validators;

namespace PolarisGateway.Functions
{
    public abstract class BasePolarisFunction
    {
        private readonly ILogger _logger;
        private readonly IAuthorizationValidator _tokenValidator;

        protected BasePolarisFunction(ILogger logger, IAuthorizationValidator tokenValidator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenValidator = tokenValidator ?? throw new ArgumentNullException(nameof(tokenValidator));
        }

        protected async Task<ValidateRequestResult> ValidateRequest(HttpRequest req, string loggingSource, string validScopes, string validRoles = "")
        {
            var result = new ValidateRequestResult();

            try
            {
                result.CurrentCorrelationId = EstablishCorrelation(req);
                // todo: only DDEI-bound requests need to have an upstream token
                result.UpstreamToken = EstablishUpstreamToken(req);
                result.AccessTokenValue = await AuthenticateRequest(req, result.CurrentCorrelationId, validScopes, validRoles);
            }
            catch (CorrelationException correlationException)
            {
                result.InvalidResponseResult = BadRequestErrorResponse(correlationException.Message, Guid.Empty, loggingSource);
            }
            catch (CpsAuthenticationException cpsAuthenticationException)
            {
                result.InvalidResponseResult = AuthenticationErrorResponse(cpsAuthenticationException.Message, result.CurrentCorrelationId, loggingSource);
            }
            catch (CpsAuthorizationException cpsAuthorizationException)
            {
                result.InvalidResponseResult = AuthorizationErrorResponse(cpsAuthorizationException.Message, result.CurrentCorrelationId, loggingSource);
            }
            catch (UpstreamAuthenticationException upstreamAuthenticationException)
            {
                result.InvalidResponseResult = UpstreamTokenErrorResponse(upstreamAuthenticationException.Message, result.CurrentCorrelationId, loggingSource);
            }

            return result;
        }

        private static Guid EstablishCorrelation(HttpRequest req)
        {
            if (!req.Headers.TryGetValue("Correlation-Id", out var correlationId) || string.IsNullOrWhiteSpace(correlationId))
                throw new CorrelationException();

            if (!Guid.TryParse(correlationId, out var currentCorrelationId) && currentCorrelationId != Guid.Empty)
                throw new CorrelationException();

            return currentCorrelationId;
        }

        private async Task<StringValues> AuthenticateRequest(HttpRequest req, Guid currentCorrelationId, string validScopes, string validRoles = "")
        {
            if (!req.Headers.TryGetValue(AuthenticationKeys.Authorization, out var accessTokenValue) ||
                string.IsNullOrWhiteSpace(accessTokenValue))
                throw new CpsAuthenticationException();

            var validToken = await _tokenValidator.ValidateTokenAsync(accessTokenValue, currentCorrelationId, validScopes, validRoles);
            if (!validToken)
                throw new CpsAuthorizationException();

            return accessTokenValue;
        }

        private static string EstablishUpstreamToken(HttpRequest req)
        {
            if (!req.Headers.TryGetValue(HttpHeaderKeys.UpstreamToken, out var upstreamToken) || string.IsNullOrWhiteSpace(upstreamToken))
                throw new UpstreamAuthenticationException();

            return upstreamToken;
        }

        private IActionResult AuthenticationErrorResponse(string errorMessage, Guid correlationId, string loggerSource)
        {
            _logger.LogMethodFlow(correlationId, loggerSource, errorMessage);
            return new BadRequestObjectResult(errorMessage);
        }

        protected IActionResult AuthorizationErrorResponse(string errorMessage, Guid correlationId, string loggerSource)
        {
            _logger.LogMethodFlow(correlationId, loggerSource, errorMessage);
            return new UnauthorizedObjectResult(errorMessage);
        }

        protected IActionResult UpstreamTokenErrorResponse(string errorMessage, Guid correlationId, string loggerSource)
        {
            _logger.LogMethodFlow(correlationId, loggerSource, errorMessage);
            return new BadRequestObjectResult(errorMessage);
        }

        protected IActionResult BadRequestErrorResponse(string errorMessage, Guid correlationId, string loggerSource)
        {
            _logger.LogMethodFlow(correlationId, loggerSource, errorMessage);
            return new BadRequestObjectResult(errorMessage);
        }

        protected IActionResult BadGatewayErrorResponse(string errorMessage, Guid correlationId, string loggerSource)
        {
            _logger.LogMethodFlow(correlationId, loggerSource, errorMessage);
            return new ObjectResult(errorMessage)
            {
                StatusCode = 502
            };
        }

        protected IActionResult NotFoundErrorResponse(string errorMessage, Guid correlationId, string loggerSource)
        {
            _logger.LogMethodFlow(correlationId, loggerSource, errorMessage);
            return new NotFoundObjectResult(errorMessage);
        }

        protected IActionResult InternalServerErrorResponse(Exception exception, string additionalMessage, Guid correlationId, string loggerSource)
        {
            _logger.LogMethodError(correlationId, loggerSource, additionalMessage, exception);
            return new ObjectResult(additionalMessage) { StatusCode = 500 };
        }

        protected void LogInformation(string message, Guid correlationId, string loggerSource)
        {
            _logger.LogMethodFlow(correlationId, loggerSource, message);
        }
    }
}
