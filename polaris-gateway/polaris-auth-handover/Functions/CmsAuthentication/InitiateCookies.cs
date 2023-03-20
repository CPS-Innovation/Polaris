using System.Net;
using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Constants;
using Common.Logging;
using Ddei.Domain.CaseData.Args;
using Ddei.Services.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PolarisDomain.Functions;
using PolarisGateway;

namespace PolarisAuthHandover.Functions.CmsAuthentication
{
    public class InitiateCookies : BaseFunction
    {
        private static string[] WhitelistedCookieNameRoots = new[] {
          "ASP.NET_SessionId",
          "UID",
          "WindowID",
          "CMSUSER", // the cookie name itself is not fixed e.g. CMSUSER246814=foo
          ".CMSAUTH",
          "BIGipServer" // the cookie name itself is not fixed e.g. BIGipServer~ent-s221~Cblahblahblah...=foo
        };

        private readonly ILogger<InitiateCookies> _logger;
        private readonly ICmsModernTokenService _cmsModernTokenService;

        public InitiateCookies(
            ILogger<InitiateCookies> logger,
            ICmsModernTokenService cmsModernTokenService) :
         base(logger)
        {
            _logger = logger;
            _cmsModernTokenService = cmsModernTokenService;
        }

        [FunctionName("Init")]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "init")] HttpRequest req,
            ILogger log)
        {
            Guid currentCorrelationId = Guid.NewGuid();
            const string loggingName = "Init - Run";

            try
            {
                var returnUrl = req.Query[CmsAuthConstants.PolarisUiQueryParamName];
                // returnUrl is mandatory to the flow, if not here then we are being misused
                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    throw new ArgumentNullException(CmsAuthConstants.PolarisUiQueryParamName);
                }

                var cookiesString = req.Query[CmsAuthConstants.CookieQueryParamName];
                var whitelistedCookies = WhitelistCookies(cookiesString);
                // cookies as passed in the query could be legitimately empty, but only do more work if they have been passed                
                if (!string.IsNullOrWhiteSpace(whitelistedCookies))
                {
                    var cmsToken = await GetCmsModernToken(whitelistedCookies, currentCorrelationId, loggingName);
                    AppendPolarisAuthCookie(req, whitelistedCookies, cmsToken);
                }

                return new RedirectResult(returnUrl);
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    ArgumentNullException => BadRequestErrorResponse(exception.Message, currentCorrelationId, loggingName),
                    _ => HandleUnhandledException(exception, currentCorrelationId, loggingName)
                };
            }
        }

        private async Task<string> GetCmsModernToken(string cmsCookiesString, Guid currentCorrelationId, string loggingName)
        {
            var cmsToken = await _cmsModernTokenService.GetCmsModernToken(new CmsCaseDataArg
            {
                CorrelationId = currentCorrelationId,
                CmsAuthValues = $"{{Cookies: \"{cmsCookiesString}\"}}"
            });

            return cmsToken;
        }

        private string WhitelistCookies(string cookieString)
        {
            if (string.IsNullOrWhiteSpace(cookieString))
            {
                return cookieString;
            }

            return cookieString
                    .Split(" ")
                    .Where(cookie => WhitelistedCookieNameRoots.Any(whitelistedCookieNameRoot => cookie.StartsWith(whitelistedCookieNameRoot)))
                    .Aggregate((curr, next) => $"{curr} {next}");
        }

        private void AppendPolarisAuthCookie(HttpRequest req, string cmsCookiesString, string cmsToken)
        {
            var cookiesString = $"{{Cookies: \"{cmsCookiesString}\", Token: \"{cmsToken}\"}}";

            var cookieOptions = req.IsHttps
            ? new CookieOptions
            {
                Path = "/api/",
                HttpOnly = true,
                Secure = true,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None
            }
            : new CookieOptions
            {
                Path = "/api/",
                HttpOnly = true,
            };

            req.HttpContext.Response.Cookies.Append(HttpHeaderKeys.CmsAuthValues, cookiesString, cookieOptions);
        }
    }
}
