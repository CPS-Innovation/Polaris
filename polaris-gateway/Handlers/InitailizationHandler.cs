using Common.Telemetry.Wrappers.Contracts;
using Microsoft.AspNetCore.Http;
using PolarisGateway.Domain.Validators;
using Common.Extensions;
using Common.Constants;
using PolarisGateway.Exceptions;

namespace PolarisGateway.Handlers;

public class InitializationHandler : IInitializationHandler
{
    private readonly IAuthorizationValidator _tokenValidator;
    private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;

    public InitializationHandler(
            IAuthorizationValidator tokenValidator,
            ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
    {

        _tokenValidator = tokenValidator ?? throw new ArgumentNullException(nameof(tokenValidator));
        _telemetryAugmentationWrapper = telemetryAugmentationWrapper ?? throw new ArgumentNullException(nameof(telemetryAugmentationWrapper));
    }

    public async Task<(Guid, string)> Initialize(HttpRequest req)
    {
        var correlationId = req.Headers.GetCorrelationId();
        _telemetryAugmentationWrapper.RegisterCorrelationId(correlationId);

        var username = await AuthenticateRequest(req, correlationId);
        // Important that we register the telemetry values we need to as soon as we have called AuthenticateRequest.
        //  We are adding our user identity in to the AppInsights logs, so best to do this before
        //  e.g. EstablishCmsAuthValues throws on missing cookies thereby preventing us from logging the user identity.
        _telemetryAugmentationWrapper.RegisterUserName(username);

        var cmsAuthValues = EstablishCmsAuthValues(req);
        _telemetryAugmentationWrapper.RegisterCmsUserId(cmsAuthValues.ExtractCmsUserId());
        return (correlationId, cmsAuthValues);
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