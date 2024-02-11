using Common.Constants;
using Common.Extensions;
using Common.Logging;
using PolarisGateway.Domain.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolarisGateway.Domain.Exceptions;
using Common.Telemetry.Wrappers.Contracts;
using Common.Domain.Exceptions;
using Ddei.Exceptions;

namespace PolarisGateway.Functions
{
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]

    // Might as well have 500 here too
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public abstract class BasePolarisFunction
    {
        private readonly ILogger _logger;
        private readonly IAuthorizationValidator _tokenValidator;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;
        protected Guid CorrelationId { get; set; }
        protected string LoggingSource { get; set; }
        protected string CmsAuthValues { get; set; }

        protected BasePolarisFunction(ILogger logger, IAuthorizationValidator tokenValidator, ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenValidator = tokenValidator ?? throw new ArgumentNullException(nameof(tokenValidator));
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper ?? throw new ArgumentNullException(nameof(telemetryAugmentationWrapper));
            LoggingSource = GetType().Name;
        }

        // todo: annotations about what this throws
        protected async Task Initiate(HttpRequest req)
        {
            // todo: can we get this from nameof(superclass) or something
            CorrelationId = req.Headers.GetCorrelation();

            var username = await AuthenticateRequest(req, CorrelationId);
            // Important that we register the telemetry values we need to as soon as we have called AuthenticateRequest.
            //  We are adding our user identity in to the AppInsights logs, so best to do this before
            //  e.g. EstablishCmsAuthValues throws on missing cookies thereby preventing us from logging the user identity.
            _telemetryAugmentationWrapper.RegisterUserName(username);

            CmsAuthValues = EstablishCmsAuthValues(req);
            _telemetryAugmentationWrapper.RegisterCmsUserId(CmsAuthValues.ExtractCmsUserId());
        }

        protected IActionResult HandleUnhandledException(Exception ex, string additionalMessage = "")
        {
            _logger.LogMethodError(CorrelationId, LoggingSource, additionalMessage, ex);

            // todo: PipelineClient exceptions
            var statusCode = ex switch
            {
                ArgumentNullException or BadRequestException _ => 400,
                CpsAuthenticationException _ => 401,
                // must be 403, client will react to a 403 to trigger reauthentication
                CaseDataServiceException or CmsAuthenticationException _ => 403,
                _ => 500,
            };

            return new ObjectResult(ex.Message) { StatusCode = statusCode };
        }

        private async Task<string> AuthenticateRequest(HttpRequest req, Guid correlationId)
        {
            if (!req.Headers.TryGetValue(OAuthSettings.Authorization, out var accessTokenValue) ||
                string.IsNullOrWhiteSpace(accessTokenValue))
                throw new CpsAuthenticationException();

            var validateTokenResult = await _tokenValidator.ValidateTokenAsync(accessTokenValue, correlationId, ValidRoles.UserImpersonation);
            if (!validateTokenResult.IsValid)
                throw new CpsAuthenticationException();

            return validateTokenResult.UserName;
        }

        private static string EstablishCmsAuthValues(HttpRequest req)
        {
            if (!req.Cookies.TryGetValue(HttpHeaderKeys.CmsAuthValues, out var cmsAuthValues) || string.IsNullOrWhiteSpace(cmsAuthValues))
                throw new CmsAuthenticationException();

            return cmsAuthValues;
        }
    }
}
