using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PolarisGateway;
using PolarisGateway.Domain.CaseData.Args;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Factories;
using PolarisGateway.Services;

namespace PolarisAuthHandover.Functions.CmsAuthentication
{
    public class InitiateCookies : BaseFunction
    {
        private readonly ILogger<InitiateCookies> _logger;
        private readonly ICmsModernTokenService _cmsModernTokenService;
        private readonly ICmsAuthValuesFactory _cmsAuthValuesFactory;
        
        public InitiateCookies(
            ILogger<InitiateCookies> logger,
            ICmsModernTokenService cmsModernTokenService,
            ICmsAuthValuesFactory cmsAuthValuesFactory) :
         base(logger)
        {
            _logger = logger;
            _cmsModernTokenService = cmsModernTokenService;
            _cmsAuthValuesFactory = cmsAuthValuesFactory;
        }

        [FunctionName("Init")]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "init")] HttpRequest req,
            ILogger log)
        {
            Guid currentCorrelationId = Guid.NewGuid();
            const string loggingName = "Init - Run";

            try
            {
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                var returnUrl = WebUtility.UrlDecode(req.Query["q"]);
                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    throw new ArgumentNullException("q");
                }

                var cookiesString = WebUtility.UrlDecode(req.Query["cookie"]);
                var cmsToken = await GetCmsModernToken(cookiesString, currentCorrelationId, loggingName);

                AppendAuthCookies(req, cookiesString, cmsToken);

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

        private async Task<string> GetCmsModernToken(string cookiesString, Guid currentCorrelationId, string loggingName)
        {
            if (string.IsNullOrWhiteSpace(cookiesString))
            {
                //  initial idea: if we do not have cookies, lets just return to the UI and let it deal with what it does next
                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Not obtaining Cms Modern token as cookies not found");
                return string.Empty;
            }

            _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Obtaining Cms Modern token");
            var cmsToken = await _cmsModernTokenService.GetCmsModernToken(new CaseDataArg
            {
                CorrelationId = currentCorrelationId,
                CmsAuthValues = _cmsAuthValuesFactory.SerializeCmsAuthValues(cookiesString),
            });

            _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Cms Modern token found");
            return cmsToken;
        }

        private void AppendAuthCookies(HttpRequest req, string cookiesString, string cmsToken)
        {
            req.HttpContext.Response.Cookies.Append(
              HttpHeaderKeys.CmsAuthValues,
              _cmsAuthValuesFactory.SerializeCmsAuthValues(cookiesString, cmsToken),
              new CookieOptions
              {
                  HttpOnly = true,
                  Path = "/api/"
              }
            );
        }
    }
}
