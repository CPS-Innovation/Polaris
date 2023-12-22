using Common.Domain.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace Gateway.Common.Extensions
{
    public static class HttpRequestExtensions
    {
        public static async Task<ValidatableRequest<T>> GetJsonBody<T, TV>(this HttpRequest request)
            where TV : AbstractValidator<T>, new()
        {
            var (requestObject, requestJson) = await request.GetJsonBody<T>();
            var validator = new TV();
            var validationResult = await validator.ValidateAsync(requestObject);

            if (!validationResult.IsValid)
            {
                return new ValidatableRequest<T>
                {
                    Value = requestObject,
                    IsValid = false,
                    Errors = validationResult.Errors,
                    RequestJson = requestJson
                };
            }

            return new ValidatableRequest<T>
            {
                Value = requestObject,
                IsValid = true,
                RequestJson = requestJson
            };
        }

        public static async Task<(T, string)> GetJsonBody<T>(this HttpRequest request)
        {
            var requestBody = await request.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<T>(requestBody);
            return (requestObject, requestBody);
        }
    }
}
