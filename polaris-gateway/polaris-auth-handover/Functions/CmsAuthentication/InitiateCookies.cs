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
using PolarisGateway;
using DdeiClient.Services.Contracts;
using Common.Configuration;
using Common.Wrappers.Contracts;
using Common.Domain.Extensions;
using Common.Telemetry.Wrappers.Contracts;

namespace PolarisAuthHandover.Functions.CmsAuthentication
{
    public class InitiateCookies
    {
        private const string XForwardedForHeaderName = "X-Forwarded-For";
        private static readonly string[] WhitelistedCookieNameRoots = new[] {
          "ASP.NET_SessionId",
          "UID",
          "WindowID",
          "CMSUSER", // the cookie name itself is not fixed e.g. CMSUSER246814=foo
          ".CMSAUTH",
          "BIGipServer" // the cookie name itself is not fixed e.g. BIGipServer~ent-s221~Cblahblahblah...=foo
        };

        private readonly IDdeiClient _ddeiClient;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper;

        public InitiateCookies(
            IDdeiClient ddeiClient,
            IJsonConvertWrapper jsonConvertWrapper,
            ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        {
            _ddeiClient = ddeiClient;
            _jsonConvertWrapper = jsonConvertWrapper;
            _telemetryAugmentationWrapper = telemetryAugmentationWrapper;
        }

        [FunctionName(nameof(InitiateCookies))]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.AuthInitialisation)] HttpRequest req)
        {
            _telemetryAugmentationWrapper.RegisterClientIp(GetClientIpAddress(req));
            var currentCorrelationId = Guid.NewGuid();
            _telemetryAugmentationWrapper.RegisterCorrelationId(currentCorrelationId);
            try
            {
                return DetectAuthFlowMode(req) switch
                {
                    AuthFlowMode.PolarisAuthRedirect => await PolarisAuthRedirectMode(req, currentCorrelationId),
                    _ => await CmsLaunchMode(req, currentCorrelationId)
                };
            }
            catch (Exception)
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        private static AuthFlowMode DetectAuthFlowMode(HttpRequest req)
        {
            return !string.IsNullOrEmpty(req.Query[CmsAuthConstants.PolarisUiQueryParamName])
                ? AuthFlowMode.PolarisAuthRedirect
                : AuthFlowMode.CmsLaunch;
        }

        private async Task<IActionResult> CmsLaunchMode(HttpRequest req, Guid correlationId)
        {
            var polarisAuthCookieContent = await CommonAuthFlow(req, correlationId);

            var redirectUrl = polarisAuthCookieContent != null
                ? await BuildCmsLaunchModeRedirectUrl(req, polarisAuthCookieContent, correlationId)
                : null;

            return new RedirectResult(redirectUrl ?? CmsAuthConstants.CmsLaunchModeFallbackRedirectUrl);
        }

        private async Task<IActionResult> PolarisAuthRedirectMode(HttpRequest req, Guid correlationId)
        {
            await CommonAuthFlow(req, correlationId);

            var redirectUrl = req.Query[CmsAuthConstants.PolarisUiQueryParamName];
            return new RedirectResult(redirectUrl);
        }

        private async Task<string> CommonAuthFlow(HttpRequest req, Guid correlationId)
        {
            var whitelistedCookies = ExtractWhitelistedCookies(req);
            if (whitelistedCookies == null)
            {
                return null;
            }
            _telemetryAugmentationWrapper.RegisterCmsUserId(whitelistedCookies.ExtractCmsUserId());
            _telemetryAugmentationWrapper.RegisterLoadBalancingCookie(whitelistedCookies.ExtractLoadBalancerCookie());

            var cmsModernToken = await GetCmsModernToken(whitelistedCookies, correlationId);
            if (cmsModernToken == null)
            {
                return null;
            }

            return CreateAndAppendPolarisAuthCookie(req, whitelistedCookies, cmsModernToken);
        }

        private static string ExtractWhitelistedCookies(HttpRequest req)
        {
            string cookiesString = req.Query[CmsAuthConstants.CookieQueryParamName];
            if (string.IsNullOrWhiteSpace(cookiesString))
            {
                return null;
            }

            var whitelistedCookies = cookiesString
                .Split(" ")
                .Where(cookie => WhitelistedCookieNameRoots.Any(whitelistedCookieNameRoot => cookie.StartsWith(whitelistedCookieNameRoot)))
                .Aggregate(string.Empty, (curr, next) => $"{curr} {next}");

            if (string.IsNullOrWhiteSpace(whitelistedCookies))
            {
                return null;
            }

            return whitelistedCookies;
        }

        private async Task<string> GetCmsModernToken(string cmsCookiesString, Guid currentCorrelationId)
        {
            try
            {
                var token = await _ddeiClient.GetCmsModernToken(new DdeiCmsCaseDataArgDto
                {
                    CorrelationId = currentCorrelationId,
                    CmsAuthValues = $"{{Cookies: \"{cmsCookiesString}\"}}"
                });
                // Note 1 of 2:  two things may be happening if have got this far.
                //  a) we have new cookies that correspond to a live Modern session and we are on the happy path.
                //  b) we may have old cookies.  This means that the modern token that we have just been given looks
                //    good, but actually will be no use to the client as it has expired as far as Modern is concerned.
                //  We can't tell here which scenario unless ( todo: ) we make a representative call to Modern to see if
                //  it doesn't fail. So we just continue to set the cookie and let the client figure things out.
                return token;
            }
            catch (Exception)
            {
                // Note 2 of 2:  there is an unhappy path that leads here.  If the cookies we have got represent a 
                //  failed CMS login or a no longer recognised CMS Classic session then the attempt to get the Modern
                //  token will throw.  In this case just redirect back to the client, no point even setting the cookie.

                // todo: be more granular and only catch an auth exception. Although if we are going to blow up
                //  here if other than an auth fail, we may need better UX or messaging.
                return null;
            }
        }

        private static string CreateAndAppendPolarisAuthCookie(HttpRequest req, string cmsCookiesString, string cmsToken)
        {
            var polarisAuthCookieContent = $"{{Cookies: \"{cmsCookiesString}\", Token: \"{cmsToken}\"}}";

            var cookieOptions = req.IsHttps
                ? new CookieOptions
                {
                    Path = "/api/",
                    HttpOnly = true,
                    // in production we are https so we need to be restrictive with cookie characteristics 
                    Secure = true,
                    SameSite = SameSiteMode.None
                }
                : new CookieOptions
                {
                    Path = "/api/",
                    HttpOnly = true,
                    // in production we are on http so *have* to be lax with cookie characteristics
                };

            req.HttpContext.Response.Cookies.Append(HttpHeaderKeys.CmsAuthValues, polarisAuthCookieContent, cookieOptions);

            return polarisAuthCookieContent;
        }

        private async Task<string> BuildCmsLaunchModeRedirectUrl(HttpRequest req, string polarisCmsAuthValues, Guid currentCorrelationId)
        {
            var cmsRedirectParam = req.Query[CmsAuthConstants.CmsRedirectQueryParamName];
            if (string.IsNullOrWhiteSpace(cmsRedirectParam))
            {
                return null;
            }

            var decodedCmsRedirectParam = cmsRedirectParam.ToString().UrlDecodeString();
            var cmsParamObject = _jsonConvertWrapper.DeserializeObject<CMSParamObject>(decodedCmsRedirectParam);
            if (cmsParamObject == null)
            {
                return null;
            }

            var caseId = cmsParamObject.CaseId;
            if (caseId == 0)
            {
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

                return $"{CmsAuthConstants.CmsLaunchModeUiRootUrl}/{urnLookupResponse.Urn}/{caseId}";
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetClientIpAddress(HttpRequest req)
        {
            return req.Headers[XForwardedForHeaderName].FirstOrDefault();
        }

        private class CMSParamObject
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
    }
}
