using Common.Constants;
using Common.Exceptions;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Linq;

namespace Common.Extensions;

public static class HttpRequestDataExtensions
{
    public static Guid EstablishCorrelation(this HttpRequestData req)
    {
        if (req.Headers is null)
        {
            throw new ArgumentNullException(nameof(HttpRequestData.Headers));
        }

        if (req.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIds) &&
            correlationIds.Any() &&
            Guid.TryParse(correlationIds.First(), out var parsedCorrelationId) &&
            parsedCorrelationId != Guid.Empty)
        {
            return parsedCorrelationId;
        }
        else
        {
            throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(HttpRequestData.Headers));
        }
    }

    public static string EstablishCmsAuthValuesFromCookies(this HttpRequestData req)
    {
        var cmsAuthValues = req.Cookies.FirstOrDefault(c => c.Name.Equals(HttpHeaderKeys.CmsAuthValues));
        return cmsAuthValues?.Value;
    }
}
