using Common.Extensions;
using Common.Telemetry;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using PolarisGateway.Validators;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Common.Exceptions;
using Common.Configuration;
using Common.Constants;

namespace PolarisGateway.Middleware;

public sealed partial class RequestValidationMiddleware(
    ITelemetryAugmentationWrapper telemetryAugmentationWrapper,
    IAuthorizationValidator authorizationValidator)
    : IFunctionsWorkerMiddleware
{
    private const int MockUserUserId = int.MinValue;

    private readonly string[] _unauthenticatedRoutes = ["/api/status", "/api/login-full-cookie", "/api/login", "/api/init", "/api/polaris"];
    private readonly string[] _nonCmsAuthenticatedRoutes = [RestApi.CaseTracker, RestApi.CaseSearchCount, RestApi.CaseSearch];//"/api/login-full-cookie", , "/api/login", "/api/init"];

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var requestData = await context.GetHttpRequestDataAsync();

        var correlationId = Guid.NewGuid();

        // TimeEnd('/') because e.g. /api/init and /api/init/ are both valid paths for a client to use
        //  to get to InitiateCookies. We want both to match the /api/init _unauthenticatedRoutes entry.
        if (!_unauthenticatedRoutes.Any(requestData.Url.LocalPath.TrimEnd('/').Equals))
        {
            correlationId = requestData.EstablishCorrelation();
            telemetryAugmentationWrapper.RegisterCorrelationId(correlationId);

            var username = await AuthenticateRequest(requestData, correlationId);
            // Important that we register the telemetry values we need to as soon as we have called AuthenticateRequest.
            //  We are adding our user identity in to the AppInsights logs, so best to do this before
            //  e.g. EstablishCmsAuthValuesFromCookies throws on missing cookies thereby preventing us from logging the user identity.
            telemetryAugmentationWrapper.RegisterUserName(username);

            if (!_nonCmsAuthenticatedRoutes.Any(requestData.Url.LocalPath.TrimEnd('/').Equals))
            {
                var cmsAuthValues = requestData.EstablishCmsAuthValuesFromCookies();
                var cmsUserId = cmsAuthValues.ExtractCmsUserId();
                var isMockUser = cmsUserId == MockUserUserId;

                telemetryAugmentationWrapper.RegisterCmsUserId(cmsUserId);

                if (isMockUser)
                {
                    telemetryAugmentationWrapper.RegisterIsMockUser(true);
                }
            }
        }

        await next(context);

        context.GetHttpResponseData()?.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
    }

    private async Task<string> AuthenticateRequest(HttpRequestData req, Guid correlationId)
    {
        if (!req.Headers.TryGetValues(OAuthSettings.Authorization, out var accessTokenValues) ||
            string.IsNullOrWhiteSpace(accessTokenValues.First()))
        {
            throw new CpsAuthenticationException();
        }

        var validateTokenResult = await authorizationValidator.ValidateTokenAsync(accessTokenValues.First(), correlationId, ValidRoles.UserImpersonation);

        return validateTokenResult.IsValid ?
            validateTokenResult.UserName :
            throw new CpsAuthenticationException();
    }
}
