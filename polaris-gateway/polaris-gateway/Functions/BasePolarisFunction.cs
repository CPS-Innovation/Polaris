using Common.Constants;
using Common.Logging;
using Common.Validators.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using PolarisGateway.Domain.Exceptions;
using PolarisGateway.Domain.Validation;
using PolarisGateway.Wrappers;
using System;
using System.Net;
using System.Threading.Tasks;

namespace PolarisGateway.Functions
{
    public abstract class BasePolarisFunction
    {
        private readonly ILogger _logger;
        private readonly IAuthorizationValidator _tokenValidator;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;

        protected BasePolarisFunction(ILogger logger, IAuthorizationValidator tokenValidator, ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenValidator = tokenValidator ?? throw new ArgumentNullException(nameof(tokenValidator));
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper;
        }

        protected async Task<ValidateRequestResult> ValidateRequest(HttpRequest req, string loggingSource, string validScopes, string validRoles = "")
        {
            var result = new ValidateRequestResult();

            try
            {
                result.CurrentCorrelationId = EstablishCorrelation(req);
                var (username, accessToken) = await AuthenticateRequest(req, result.CurrentCorrelationId, validScopes, validRoles);
                result.AccessTokenValue = accessToken;
                // Important that we call RegisterCriticalTelemetry as soon as we have called AuthenticateRequest.
                //  We are adding our user identity in to the AppInsights logs, so best to do this before
                //  e.g. EstablishCmsAuthValues throws on missing cookies thereby preventing us from logging the user identity.
                _telemetryAugmentationWrapper.AugmentRequestTelemetry(username, result.CurrentCorrelationId);

                // todo: only DDEI-bound requests need to have a cms auth values
                result.CmsAuthValues = EstablishCmsAuthValues(req);
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
            catch (CmsAuthenticationException cmsAuthenticationException)
            {
                result.InvalidResponseResult = CmsAuthValuesErrorResponse(cmsAuthenticationException.Message, result.CurrentCorrelationId, loggingSource);
            }

            return result;
        }
        
        private static Guid EstablishCorrelation(HttpRequest req)
        {
            if (!req.Headers.TryGetValue(HttpHeaderKeys.CorrelationId, out var correlationId) || string.IsNullOrWhiteSpace(correlationId))
                throw new CorrelationException();

            if (!Guid.TryParse(correlationId, out var currentCorrelationId) && currentCorrelationId != Guid.Empty)
                throw new CorrelationException();

            return currentCorrelationId;
        }

        private async Task<(String, StringValues)> AuthenticateRequest(HttpRequest req, Guid currentCorrelationId, string validScopes, string validRoles = "")
        {
            if (!req.Headers.TryGetValue(OAuthSettings.Authorization, out var accessTokenValue) ||
                string.IsNullOrWhiteSpace(accessTokenValue))
                throw new CpsAuthenticationException();

            var validateTokenResult = await _tokenValidator.ValidateTokenAsync(accessTokenValue, currentCorrelationId, validScopes, validRoles);
            if (!validateTokenResult.IsValid)
                throw new CpsAuthorizationException();

            return (validateTokenResult.UserName, accessTokenValue);
        }

        private static string EstablishCmsAuthValues(HttpRequest req)
        {
            if (!req.Cookies.TryGetValue(HttpHeaderKeys.CmsAuthValues, out var cmsAuthValues) || string.IsNullOrWhiteSpace(cmsAuthValues))
                throw new CmsAuthenticationException();

            return cmsAuthValues;
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

        protected IActionResult CmsAuthValuesErrorResponse(string errorMessage, Guid correlationId, string loggerSource)
        {
            _logger.LogMethodFlow(correlationId, loggerSource, errorMessage);
            return new ObjectResult(errorMessage)
            {
                // client will react to a 403 to trigger reauthentication
                StatusCode = 403
            };
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

        protected IActionResult InternalServerErrorResponse(Exception exception, string additionalMessage, Guid correlationId, string loggerSource, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            _logger.LogMethodError(correlationId, loggerSource, additionalMessage, exception);
            return new ObjectResult(additionalMessage) { StatusCode = (int)statusCode };
        }

        protected void LogInformation(string message, Guid correlationId, string loggerSource)
        {
            _logger.LogMethodFlow(correlationId, loggerSource, message);
        }
    }
}
