using Common.Domain.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Extensions
{
    public static class HttpRequestExtensions
    {
        private const string XForwardedForHeaderName = "X-Forwarded-For";

        private const string EmptyClientIpAddress = "0.0.0.0";

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
    }
}
