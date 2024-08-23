using Common.Telemetry;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using PolarisGateway.Validators;

namespace PolarisGateway.Functions;

public abstract class BaseFunction(ITelemetryClient telemetryClient)
{
    protected IActionResult SendTelemetryAndReturn(BaseTelemetryEvent telemetryEvent, IActionResult result)
    {
        telemetryClient.TrackEvent(telemetryEvent);
        return result;
    }

    protected IActionResult SendTelemetryAndReturnBadRequest(BaseTelemetryEvent telemetryEvent)
    {
        telemetryClient.TrackEvent(telemetryEvent);
        return new BadRequestResult();
    }
    
    protected static async Task<ValidatableRequest<T>> GetJsonBody<T, V>(HttpRequest request)
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