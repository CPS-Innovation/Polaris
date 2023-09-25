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
using PolarisAuthHandover.Domain.Dto;
using PolarisAuthHandover.Domain.Enums;

namespace PolarisAuthHandover.Functions.CmsAuthentication
{
    public class InitiateCookies
    {
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
            _telemetryAugmentationWrapper.RegisterClientIp(req.GetClientIpAddress());
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
            var cmsAuthValues = await CommonAuthFlow(req, correlationId);

            var redirectUrl = cmsAuthValues != null
                ? await BuildCmsLaunchModeRedirectUrl(req, cmsAuthValues, correlationId)
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
            _telemetryAugmentationWrapper.RegisterLoadBalancingCookie(whitelistedCookies.ExtractLoadBalancerCookies());

            var fullCmsAuthValues = await GetFullCmsAuthValues(req, whitelistedCookies, correlationId);
            if (fullCmsAuthValues == null)
            {
                return null;
            }

            AppendPolarisAuthCookie(req, fullCmsAuthValues);

            return fullCmsAuthValues;
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
                .Where(cookie => AuthHandoverConstants.WhitelistedCookieNameRoots
                    .Any(whitelistedCookieNameRoot => cookie.StartsWith(whitelistedCookieNameRoot)))
                    .Aggregate(string.Empty, (curr, next) => $"{curr} {next}")
                    .Trim();

            if (string.IsNullOrWhiteSpace(whitelistedCookies))
            {
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

        private static void AppendPolarisAuthCookie(HttpRequest req, string cmsAuthValues)
        {
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

            req.HttpContext.Response.Cookies.Append(HttpHeaderKeys.CmsAuthValues, cmsAuthValues, cookieOptions);
        }

        private async Task<string> BuildCmsLaunchModeRedirectUrl(HttpRequest req, string polarisCmsAuthValues, Guid currentCorrelationId)
        {
            var cmsRedirectParam = req.Query[CmsAuthConstants.CmsRedirectQueryParamName];
            if (string.IsNullOrWhiteSpace(cmsRedirectParam))
            {
                return null;
            }

            var decodedCmsRedirectParam = cmsRedirectParam.ToString().UrlDecodeString();
            var cmsParamObject = _jsonConvertWrapper.DeserializeObject<CmsHandoverParams>(decodedCmsRedirectParam);
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
    }
}
