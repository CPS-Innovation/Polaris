using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using polaris_common.Domain.Validation;

namespace polaris_common.Extensions
{
    public static class HttpRequestExtensions
    {
        private const string XForwardedForHeaderName = "X-Forwarded-For";

        private const string EmptyClientIpAddress = "0.0.0.0";

        private const string CmsAuthCookieName = ".CMSAUTH";

        private const string CmsAuthCookieContentReplacementText = "REDACTED";

        public static async Task<ValidatableRequest<T>> GetJsonBody<T, V>(this HttpRequest request)
            where V : AbstractValidator<T>, new()
        {
            var requestObject = await request.GetJsonBody<T>();
            var validator = new V();
            var validationResult = await validator.ValidateAsync(requestObject);

            if (!validationResult.IsValid)
            {
                return new ValidatableRequest<T>
                {
                    Value = requestObject,
                    IsValid = false,
                    Errors = validationResult.Errors
                };
            }

            return new ValidatableRequest<T>
            {
                Value = requestObject,
                IsValid = true
            };
        }

        public static async Task<T> GetJsonBody<T>(this HttpRequest request)
        {
            var requestBody = await request.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(requestBody);
        }

        public static string GetClientIpAddress(this HttpRequest req)
        {
            return req.Headers[XForwardedForHeaderName]
                .FirstOrDefault()
                ?.Split(new char[] { ',' })
                .FirstOrDefault()
                ?.Split(new char[] { ':' })
                .FirstOrDefault()
                ?? EmptyClientIpAddress;
        }

        public static string GetLogSafeQueryString(this HttpRequest req)
        {
            var queryString = req.QueryString.ToString();
            // we are trying not log the full .CMSAUTH cookie so we're not logging auth info
            return Regex.Replace(
                queryString,
                $"({CmsAuthCookieName})(=|%3D)(.*?)(;|%3B|$)",
                $"$1$2{CmsAuthCookieContentReplacementText}$4",
                RegexOptions.None,
                TimeSpan.FromSeconds(1));
        }
    }
}
