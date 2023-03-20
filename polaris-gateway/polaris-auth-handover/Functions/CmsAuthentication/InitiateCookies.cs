using System;
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
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                var returnUrl = Uri.UnescapeDataString(req.Query[CmsAuthConstants.PolarisUiQueryParamName]);
                // returnUrl is mandatory to the flow, if not here then we are being misused
                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    throw new ArgumentNullException(CmsAuthConstants.PolarisUiQueryParamName);
                }

                var cookiesString = Uri.UnescapeDataString(req.Query[CmsAuthConstants.CookieQueryParamName]);
                // cookiesString could be legitimatly empty, but only do more work if it cookie have been passed
                if (!string.IsNullOrWhiteSpace(cookiesString))
                {
                    var cmsToken = await GetCmsModernToken(cookiesString, currentCorrelationId, loggingName);
                    AppendPolarisAuthCookie(req, cookiesString, cmsToken);
                }

                return new RedirectResult(returnUrl);
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    ArgumentNullException => BadRequestErrorResponse(exception.Message, currentCorrelationId, loggingName),
                    _ => InternalServerErrorResponse(exception, "An unhandled exception occurred.", currentCorrelationId, loggingName)
                };
            }
            finally
            {
                // todo: should we be logging the incoming info (security?)
                _logger.LogMethodExit(currentCorrelationId, loggingName, req.QueryString.Value);
            }
        }

        private async Task<string> GetCmsModernToken(string cmsCookiesString, Guid currentCorrelationId, string loggingName)
        {
            if (string.IsNullOrWhiteSpace(cmsCookiesString))
            {
                //  initial idea: if we do not have cookies, lets just return to the UI and let it deal with what it does next
                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Not obtaining Cms Modern token as cookies not found");
                return string.Empty;
            }

            _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Obtaining Cms Modern token");
            var cmsToken = await _cmsModernTokenService.GetCmsModernToken(new CmsCaseDataArg
            {
                CorrelationId = currentCorrelationId,
                CmsAuthValues = $"{{Cookies: \"{cmsCookiesString}\"}}"
            });

            _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Cms Modern token found");
            return cmsToken;
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
