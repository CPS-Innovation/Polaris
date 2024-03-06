using Common.Constants;
using Common.Extensions;
using PolarisGateway.Extensions;
using Ddei.Domain.CaseData.Args;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using DdeiClient.Services;
using Common.Configuration;
using Common.Wrappers.Contracts;
using Common.Domain.Extensions;
using Common.Telemetry.Wrappers.Contracts;
using Common.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Ddei.Factories;

namespace PolarisGateway.Functions.CmsAuthentication
{
    public class InitiateCookies
    {
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;
        private readonly ILogger<InitiateCookies> _logger;

        public InitiateCookies(
            IDdeiClient ddeiClient,
            IDdeiArgFactory ddeiArgFactory,
            IJsonConvertWrapper jsonConvertWrapper,
            ITelemetryAugmentationWrapper telemetryAugmentationWrapper,
            ILogger<InitiateCookies> logger)
        {
            _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
            _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper ?? throw new ArgumentNullException(nameof(telemetryAugmentationWrapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [FunctionName(nameof(InitiateCookies))]
        [ProducesResponseType(StatusCodes.Status301MovedPermanently)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.AuthInitialisation)] HttpRequest req)
        {
            var correlationId = Guid.NewGuid();
            _telemetryAugmentationWrapper.RegisterClientIp(req.GetClientIpAddress());
            _telemetryAugmentationWrapper.RegisterCorrelationId(correlationId);

            _logger.LogMethodFlow(correlationId, nameof(Get), $"Referrer: {req.Headers[HeaderNames.Referer]}");
            _logger.LogMethodFlow(correlationId, nameof(Get), $"Query: {req.GetLogSafeQueryString()}");

            try
            {
                var authFlowMode = DetectAuthFlowMode(req);
                _logger.LogMethodFlow(correlationId, nameof(Get), $"{authFlowMode} detected");

                var polarisCookie = await ApplyPolarisAuthCookie(req, correlationId);

                var redirectUrl = authFlowMode switch
                {
                    AuthFlowMode.PolarisAuthRedirect => await GetPolarisAuthRedirectModeRedirectUrl(req, polarisCookie),
                    _ => await GetCmsLaunchModeRedirectUrl(req, polarisCookie, correlationId)
                };

                _logger.LogMethodFlow(correlationId, nameof(Get), $"Redirecting to {redirectUrl}");
                return new RedirectResult(redirectUrl);
            }
            catch (Exception ex)
            {
                LogException(nameof(Get), ex, correlationId);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
        private async Task<string> ApplyPolarisAuthCookie(HttpRequest req, Guid correlationId)
        {
            var whitelistedCookies = ExtractWhitelistedCookies(req, correlationId);
            if (whitelistedCookies == null)
            {
                LogEmptyValue(nameof(ApplyPolarisAuthCookie), nameof(whitelistedCookies), correlationId);
                return null;
            }
            _telemetryAugmentationWrapper.RegisterCmsUserId(whitelistedCookies.ExtractCmsUserId());
            _telemetryAugmentationWrapper.RegisterLoadBalancingCookie(whitelistedCookies.ExtractLoadBalancerCookies());

            var fullCmsAuthValues = await GetFullCmsAuthValues(req, whitelistedCookies, correlationId);
            if (fullCmsAuthValues == null)
            {
                LogEmptyValue(nameof(ApplyPolarisAuthCookie), nameof(fullCmsAuthValues), correlationId);
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
                LogEmptyValue(nameof(GetCmsLaunchModeRedirectUrl), nameof(polarisCookie), correlationId);
                return CmsAuthConstants.CmsLaunchModeFallbackRedirectUrl;
            }

            var redirectUrl = await BuildCmsLaunchModeRedirectUrl(req, polarisCookie, correlationId);

            if (redirectUrl == null)
            {
                LogEmptyValue(nameof(GetCmsLaunchModeRedirectUrl), nameof(redirectUrl), correlationId);
                return CmsAuthConstants.CmsLaunchModeFallbackRedirectUrl;
            }
            else
            {
                return redirectUrl;
            }
        }

        private Task<string> GetPolarisAuthRedirectModeRedirectUrl(HttpRequest req, string polarisCookie)
        {
            return Task.FromResult(req.Query[CmsAuthConstants.PolarisUiQueryParamName].ToString());
        }

        private string ExtractWhitelistedCookies(HttpRequest req, Guid correlationId)
        {
            string cookiesString = req.Query[CmsAuthConstants.CookieQueryParamName];
            if (string.IsNullOrWhiteSpace(cookiesString))
            {
                LogEmptyValue(nameof(ExtractWhitelistedCookies), nameof(cookiesString), correlationId);
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
                LogEmptyValue(nameof(ExtractWhitelistedCookies), nameof(whitelistedCookies), correlationId);
                return null;
            }

            return whitelistedCookies;
        }

        private async Task<string> GetFullCmsAuthValues(HttpRequest req, string cmsCookiesString, Guid correlationId)
        {
            try
            {
                var partialCmsAuthValues = $"{{Cookies: \"{cmsCookiesString}\", UserIpAddress: \"{req.GetClientIpAddress()}\"}}";

                var fullCmsAuthValues = await _ddeiClient.GetFullCmsAuthValuesAsync(
                    _ddeiArgFactory.CreateCmsAuthValuesArg(partialCmsAuthValues, correlationId)
                );
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
                LogException(nameof(GetFullCmsAuthValues), ex, correlationId);
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

        private async Task<string> BuildCmsLaunchModeRedirectUrl(HttpRequest req, string polarisCmsAuthValues, Guid correlationId)
        {
            var cmsRedirectParam = req.Query[CmsAuthConstants.CmsRedirectQueryParamName];
            if (string.IsNullOrWhiteSpace(cmsRedirectParam))
            {
                LogEmptyValue(nameof(BuildCmsLaunchModeRedirectUrl), nameof(cmsRedirectParam), correlationId);
                return null;
            }

            var decodedCmsRedirectParam = cmsRedirectParam.ToString().UrlDecodeString();
            var cmsParamObject = _jsonConvertWrapper.DeserializeObject<CmsHandoverParams>(decodedCmsRedirectParam);
            if (cmsParamObject == null)
            {
                LogEmptyValue(nameof(BuildCmsLaunchModeRedirectUrl), nameof(cmsParamObject), correlationId);
                return null;
            }

            var caseId = cmsParamObject.CaseId;
            if (caseId == 0)
            {
                LogEmptyValue(nameof(BuildCmsLaunchModeRedirectUrl), nameof(caseId), correlationId);
                return null;
            }

            try
            {
                var urnLookupResponse = await _ddeiClient.GetUrnFromCaseIdAsync(new DdeiCmsCaseIdArgDto
                {
                    CorrelationId = correlationId,
                    CmsAuthValues = polarisCmsAuthValues,
                    CaseId = caseId
                });

                return $"{CmsAuthConstants.CmsLaunchModeUiRootUrl}/{urnLookupResponse.UrnRoot}/{caseId}";
            }
            catch (Exception ex)
            {
                LogException(nameof(BuildCmsLaunchModeRedirectUrl), ex, correlationId);
                return null;
            }
        }

        private void LogEmptyValue(string methodName, string valueName, Guid correlationId)
        {
            _logger.LogMethodFlow(correlationId, methodName, $"{valueName} is empty");
        }

        private void LogException(string methodName, Exception ex, Guid correlationId)
        {
            _logger.LogMethodError(correlationId, methodName, ex.Message, ex);
        }

        private class CmsHandoverParams
        {
            public int CaseId { get; set; }
        }

        private enum AuthFlowMode
        {
            // There are two mechanisms for redirecting back to the client.  
            //  Mechanism 1: the Polaris UI has detected missing or expired auth and redirects to this endpoint with
            //    a query param that contains the URL to redirect to after auth.
            PolarisAuthRedirect,
            // Mechanism 2: we are brought here from CMS.  The scheme there is that we are passed a case id of a case.
            //  This is passed in the form of `q=%7B%22caseId%22%3A2073383%7D` where there is a fragment of JSON that
            //  contains the case id.  We need to extract this and then call Modern to get the URN to form our full
            //  redirect URL.
            CmsLaunch
        }

        private class AuthHandoverConstants
        {
            public static readonly string[] WhitelistedCookieNameRoots = new[] {
          "ASP.NET_SessionId",
          "UID",
          "WindowID",
          "CMSUSER", // the cookie name itself is not fixed e.g. CMSUSER246814=foo
          ".CMSAUTH",
          "BIGipServer" // the cookie name itself is not fixed e.g. BIGipServer~ent-s221~Cblahblahblah...=foo
        };
        }
    }
}
