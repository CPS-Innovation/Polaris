using System.Net;
using System.Text.Json;
using Common.Converters;
using Microsoft.AspNetCore.Mvc;

namespace text_extractor.Functions;

public abstract class BaseFunction
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters =
        {
            new JsonNullToEmptyStringConverter()
        }
    };

    protected JsonResult CreateJsonResult(object value)
    {
        var result = new JsonResult(value)
        {
            StatusCode = (int)HttpStatusCode.OK,
            SerializerSettings = _jsonSerializerOptions
        };

        return result;
    }
}