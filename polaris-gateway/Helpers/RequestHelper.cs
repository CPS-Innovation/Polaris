using FluentValidation;
using Microsoft.AspNetCore.Http;
using PolarisGateway.Validators;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace PolarisGateway.Helpers
{
    public static class RequestHelper
    {
        public static async Task<ValidatableRequest<T>> GetJsonBody<T, V>(HttpRequest request)
            where V : AbstractValidator<T>, new()
        {
            request.Headers.Remove("Content-Type");
            request.Headers.Append("Content-Type", "application/json");
            var requestObject = await request.ReadFromJsonAsync<T>();

            var validator = new V();
            var validationResult = await validator.ValidateAsync(requestObject);

            return new ValidatableRequest<T>
            {
                Value = requestObject,
                IsValid = validationResult.IsValid,
                RequestJson = JsonSerializer.Serialize(validationResult),
            };
        }
    }
}