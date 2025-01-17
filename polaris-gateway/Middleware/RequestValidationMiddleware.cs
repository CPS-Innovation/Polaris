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
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights;

namespace PolarisGateway.Middleware;

public sealed partial class RequestValidationMiddleware(
    IAuthorizationValidator authorizationValidator,
    Microsoft.ApplicationInsights.TelemetryClient telemetryClient)
    : IFunctionsWorkerMiddleware
{
    private const int MockUserUserId = int.MinValue;

    private readonly string[] _unauthenticatedRoutes = ["/api/status", "/api/login-full-cookie", "/api/login", "/api/init", "/api/polaris"];
    private readonly string[] _nonCmsAuthenticatedRoutes = [RestApi.CaseTracker, RestApi.CaseSearchCount, RestApi.CaseSearch];//"/api/login-full-cookie", , "/api/login", "/api/init"];

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var requestTelemetry = new RequestTelemetry();
        requestTelemetry.Start();
        var requestData = await context.GetHttpRequestDataAsync();

        var correlationId = Guid.NewGuid();

        // TimeEnd('/') because e.g. /api/init and /api/init/ are both valid paths for a client to use
        //  to get to InitiateCookies. We want both to match the /api/init _unauthenticatedRoutes entry.
        if (!_unauthenticatedRoutes.Any(requestData.Url.LocalPath.TrimEnd('/').Equals))
        {
            correlationId = requestData.EstablishCorrelation();

            var username = await AuthenticateRequest(requestData, correlationId);
            // Important that we register the telemetry values we need to as soon as we have called AuthenticateRequest.
            //  We are adding our user identity in to the AppInsights logs, so best to do this before
            //  e.g. EstablishCmsAuthValuesFromCookies throws on missing cookies thereby preventing us from logging the user identity.
            requestTelemetry.Properties[TelemetryConstants.UserCustomDimensionName] = username;

            if (!_nonCmsAuthenticatedRoutes.Any(requestData.Url.LocalPath.TrimEnd('/').Equals))
            {
                var cmsAuthValues = requestData.EstablishCmsAuthValues();
                var cmsUserId = cmsAuthValues.ExtractCmsUserId();
                var isMockUser = cmsUserId == MockUserUserId;

                requestTelemetry.Properties[TelemetryConstants.CmsUserIdCustomDimensionName] = cmsUserId.ToString();

                if (isMockUser)
                {
                    requestTelemetry.Properties[TelemetryConstants.IsMockUser] = true.ToString();
                }
            }
        }

        await next(context);

        requestTelemetry.Properties[TelemetryConstants.CorrelationIdCustomDimensionName] = correlationId.ToString();
        requestTelemetry.Name = context.FunctionDefinition.Name;
        requestTelemetry.HttpMethod = requestData.Method;
        requestTelemetry.ResponseCode = context.GetHttpResponseData()?.StatusCode.ToString() ?? string.Empty;
        requestTelemetry.Success = true;
        requestTelemetry.Url = requestData.Url;
        requestTelemetry.Stop();

        telemetryClient.TrackRequest(requestTelemetry);

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