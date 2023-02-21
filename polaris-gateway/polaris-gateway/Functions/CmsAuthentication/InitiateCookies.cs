using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolarisGateway.CaseDataImplementations.Ddei.Options;
using PolarisGateway.Domain.CaseData.Args;
using PolarisGateway.Domain.Exceptions;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Factories.Contracts;
using PolarisGateway.Services;

namespace PolarisGateway.Functions.CmsAuthentication
{
    public class InitiateCookies : BasePolarisFunction
    {
        private readonly ILogger<InitiateCookies> _logger;
        private readonly IAuthorizationValidator _tokenValidator;
        private readonly ICmsModernTokenService _cmsModernTokenService;

        private readonly ICmsAuthValuesFactory _cmsAuthValuesFactory;
        private readonly DdeiOptions _ddeiOptions;

        public InitiateCookies(
            ILogger<InitiateCookies> logger,
            IAuthorizationValidator tokenValidator,
            ICmsModernTokenService cmsModernTokenService,
            ICmsAuthValuesFactory cmsAuthValuesFactory,
            IOptions<DdeiOptions> options) :
         base(logger, tokenValidator)
        {
            _logger = logger;
            _tokenValidator = tokenValidator;
            _cmsModernTokenService = cmsModernTokenService;
            _cmsAuthValuesFactory = cmsAuthValuesFactory;
            _ddeiOptions = options.Value;
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
                    CaseDataServiceException => InternalServerErrorResponse(exception, "A case data api exception occurred.", currentCorrelationId, loggingName),
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
