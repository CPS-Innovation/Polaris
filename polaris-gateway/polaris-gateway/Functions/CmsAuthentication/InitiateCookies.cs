using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Constants;
using Common.Extensions;
using Ddei.Domain.CaseData.Args;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using DdeiClient.Services.Contracts;
using Common.Configuration;
using Common.Wrappers.Contracts;
using Common.Domain.Extensions;
using Common.Telemetry.Wrappers.Contracts;
using Common.Logging;
using PolarisGateway.Constants;
using PolarisGateway.Domain.Dto;
using PolarisGateway.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace PolarisGateway.Functions.CmsAuthentication
{
    public class InitiateCookies
    {
        private readonly IDdeiClient _ddeiClient;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;
        private readonly ILogger<InitiateCookies> _logger;
        private readonly Guid _correlationId;

        public InitiateCookies(
            IDdeiClient ddeiClient,
            IJsonConvertWrapper jsonConvertWrapper,
            ITelemetryAugmentationWrapper telemetryAugmentationWrapper,
            ILogger<InitiateCookies> logger)
        {
            _ddeiClient = ddeiClient;
            _jsonConvertWrapper = jsonConvertWrapper;
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper;
            _logger = logger;
            _correlationId = Guid.NewGuid();
        }

        [FunctionName(nameof(InitiateCookies))]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.AuthInitialisation)] HttpRequest req)
        {
            _telemetryAugmentationWrapper.RegisterClientIp(req.GetClientIpAddress());
            _telemetryAugmentationWrapper.RegisterCorrelationId(_correlationId);

            _logger.LogMethodFlow(_correlationId, nameof(Get), $"Referrer: {req.Headers[HeaderNames.Referer]}");
            _logger.LogMethodFlow(_correlationId, nameof(Get), $"Query: {req.GetLogSafeQueryString()}");

            try
            {
                var authFlowMode = DetectAuthFlowMode(req);
                _logger.LogMethodFlow(_correlationId, nameof(Get), $"{authFlowMode} detected");

                var polarisCookie = await ApplyPolarisAuthCookie(req, _correlationId);

                var redirectUrl = authFlowMode switch
                {
                    AuthFlowMode.PolarisAuthRedirect => await GetPolarisAuthRedirectModeRedirectUrl(req, polarisCookie, _correlationId),
                    _ => await GetCmsLaunchModeRedirectUrl(req, polarisCookie, _correlationId)
                };

                _logger.LogMethodFlow(_correlationId, nameof(Get), $"Redirecting to {redirectUrl}");
                return new RedirectResult(redirectUrl);
            }
            catch (Exception ex)
            {
                LogException(nameof(Get), ex);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
        private async Task<string> ApplyPolarisAuthCookie(HttpRequest req, Guid correlationId)
        {
            var whitelistedCookies = ExtractWhitelistedCookies(req);
            if (whitelistedCookies == null)
            {
                LogEmptyValue(nameof(ApplyPolarisAuthCookie), nameof(whitelistedCookies));
                return null;
            }
            _telemetryAugmentationWrapper.RegisterCmsUserId(whitelistedCookies.ExtractCmsUserId());
            _telemetryAugmentationWrapper.RegisterLoadBalancingCookie(whitelistedCookies.ExtractLoadBalancerCookies());

            var fullCmsAuthValues = await GetFullCmsAuthValues(req, whitelistedCookies, correlationId);
            if (fullCmsAuthValues == null)
            {
                LogEmptyValue(nameof(ApplyPolarisAuthCookie), nameof(fullCmsAuthValues));
                return null;
            }

            AppendPolarisAuthCookie(req, fullCmsAuthValues);

            return fullCmsAuthValues;
        }

        private static AuthFlowMode DetectAuthFlowMode(HttpRequest req)
        {
            return !string.IsNullOrEmpty(req.Query[CmsAuthConstants.PolarisUiQueryParamName])
                ? AuthFlowMode.PolarisAuthRedirect
                : AuthFlowMode.CmsLaunch;
        }

        private async Task<string> GetCmsLaunchModeRedirectUrl(HttpRequest req, string polarisCookie, Guid correlationId)
        {
            if (polarisCookie == null)
            {
                LogEmptyValue(nameof(GetCmsLaunchModeRedirectUrl), nameof(polarisCookie));
                return CmsAuthConstants.CmsLaunchModeFallbackRedirectUrl;
            }

            var redirectUrl = await BuildCmsLaunchModeRedirectUrl(req, polarisCookie, correlationId);

            if (redirectUrl == null)
            {
                LogEmptyValue(nameof(GetCmsLaunchModeRedirectUrl), nameof(redirectUrl));
                return CmsAuthConstants.CmsLaunchModeFallbackRedirectUrl;
            }
            else
            {
                return redirectUrl;
            }
        }

        private Task<string> GetPolarisAuthRedirectModeRedirectUrl(HttpRequest req, string polarisCookie, Guid correlationId)
        {
            return Task.FromResult(req.Query[CmsAuthConstants.PolarisUiQueryParamName].ToString());
        }

        private string ExtractWhitelistedCookies(HttpRequest req)
        {
            string cookiesString = req.Query[CmsAuthConstants.CookieQueryParamName];
            if (string.IsNullOrWhiteSpace(cookiesString))
            {
                LogEmptyValue(nameof(ExtractWhitelistedCookies), nameof(cookiesString));
                return null;
            }

            var whitelistedCookies = cookiesString
                .Split(" ")
                .Where(cookie => AuthHandoverConstants.WhitelistedCookieNameRoots
                    .Any(whitelistedCookieNameRoot => cookie.StartsWith(whitelistedCookieNameRoot)))
                    .Aggregate(string.Empty, (curr, next) => $"{curr} {next}")
                    .Trim();

            if (string.IsNullOrWhiteSpace(whitelistedCookies))
            {
                LogEmptyValue(nameof(ExtractWhitelistedCookies), nameof(whitelistedCookies));
                return null;
            }

            return whitelistedCookies;
        }

        private async Task<string> GetFullCmsAuthValues(HttpRequest req, string cmsCookiesString, Guid currentCorrelationId)
        {
            try
            {
                var partialCmsAuthValues = $"{{Cookies: \"{cmsCookiesString}\", UserIpAddress: \"{req.GetClientIpAddress()}\"}}";

                var fullCmsAuthValues = await _ddeiClient.GetFullCmsAuthValues(new DdeiCmsCaseDataArgDto
                {
                    CorrelationId = currentCorrelationId,
                    CmsAuthValues = partialCmsAuthValues
                });
                // Note 1 of 2:  two things may be happening if have got this far.
                //  a) we have new cookies that correspond to a live Modern session and we are on the happy path.
                //  b) we may have old cookies.  This means that the modern token that we have just been given looks
                //    good, but actually will be no use to the client as it has expired as far as Modern is concerned.
                //  We can't tell here which scenario unless ( todo: ) we make a representative call to Modern to see if
                //  it doesn't fail. So we just continue to set the cookie and let the client figure things out.
                return _jsonConvertWrapper.SerializeObject(fullCmsAuthValues);
            }
            catch (Exception ex)
            {
                LogException(nameof(GetFullCmsAuthValues), ex);
                // Note 2 of 2:  there is an unhappy path that leads here.  If the cookies we have got represent a 
                //  failed CMS login or a no longer recognised CMS Classic session then the attempt to get the Modern
                //  token will throw.  In this case just redirect back to the client, no point even setting the cookie.

                // todo: be more granular and only catch an auth exception. Although if we are going to blow up
                //  here if other than an auth fail, we may need better UX or messaging.
                return null;
            }
        }

        private static void AppendPolarisAuthCookie(HttpRequest req, string cmsAuthValues)
        {
            var cookieOptions = req.IsHttps
                ? new CookieOptions
                {
                    Path = "/api/",
                    HttpOnly = true,
                    // in production we are https so we need to be restrictive with cookie characteristics 
                    Secure = true,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None
                }
                : new CookieOptions
                {
                    Path = "/api/",
                    HttpOnly = true,
                    // in production we are on http so *have* to be lax with cookie characteristics
                };

            req.HttpContext.Response.Cookies.Append(HttpHeaderKeys.CmsAuthValues, cmsAuthValues, cookieOptions);
        }

        private async Task<string> BuildCmsLaunchModeRedirectUrl(HttpRequest req, string polarisCmsAuthValues, Guid currentCorrelationId)
        {
            var cmsRedirectParam = req.Query[CmsAuthConstants.CmsRedirectQueryParamName];
            if (string.IsNullOrWhiteSpace(cmsRedirectParam))
            {
                LogEmptyValue(nameof(BuildCmsLaunchModeRedirectUrl), nameof(cmsRedirectParam));
                return null;
            }

            var decodedCmsRedirectParam = cmsRedirectParam.ToString().UrlDecodeString();
            var cmsParamObject = _jsonConvertWrapper.DeserializeObject<CmsHandoverParams>(decodedCmsRedirectParam);
            if (cmsParamObject == null)
            {
                LogEmptyValue(nameof(BuildCmsLaunchModeRedirectUrl), nameof(cmsParamObject));
                return null;
            }

            var caseId = cmsParamObject.CaseId;
            if (caseId == 0)
            {
                LogEmptyValue(nameof(BuildCmsLaunchModeRedirectUrl), nameof(caseId));
                return null;
            }

            try
            {
                var urnLookupResponse = await _ddeiClient.GetUrnFromCaseId(new DdeiCmsCaseIdArgDto
                {
                    CorrelationId = currentCorrelationId,
                    CmsAuthValues = polarisCmsAuthValues,
                    CaseId = caseId
                });

                return $"{CmsAuthConstants.CmsLaunchModeUiRootUrl}/{urnLookupResponse.UrnRoot}/{caseId}";
            }
            catch (Exception ex)
            {
                LogException(nameof(BuildCmsLaunchModeRedirectUrl), ex);
                return null;
            }
        }

        private void LogEmptyValue(string methodName, string valueName)
        {
            _logger.LogMethodFlow(_correlationId, methodName, $"{valueName} is empty");
        }

        private void LogException(string methodName, Exception ex)
        {
            _logger.LogMethodError(_correlationId, methodName, ex.Message, ex);
        }
    }
}
