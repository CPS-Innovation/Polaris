using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using PolarisGateway.Validators;

namespace PolarisGateway.Helpers
{
    public static class RequestHelper
    {
        public static async Task<ValidatableRequest<T>> GetJsonBody<T, V>(HttpRequest request)
            where V : AbstractValidator<T>, new()
        {
            var requestJson = await request.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<T>(requestJson);

            var validator = new V();
            var validationResult = await validator.ValidateAsync(requestObject);

            return new ValidatableRequest<T>
            {
                Value = requestObject,
                IsValid = validationResult.IsValid,
                RequestJson = requestJson
            };
        }
    }
}