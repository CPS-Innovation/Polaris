using Common.Constants;
using Common.Dto.Request;
using Common.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net.Http.Headers;

namespace Common.Extensions
{
    public static class HttpRequestHeadersExtensions
    {
        public static Guid GetCorrelationId(this HttpRequestHeaders headers)
        {
            headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
            if (correlationIdValues == null)
            {
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(headers));
            }

            var correlationId = correlationIdValues.First();
            if (!Guid.TryParse(correlationId, out var currentCorrelationId) || currentCorrelationId == Guid.Empty)
            {
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", correlationId);
            }

            return currentCorrelationId;
        }

        public static Guid GetCorrelationId(this IHeaderDictionary headers)
        {
            ArgumentNullException.ThrowIfNull(headers);

            if (!headers.TryGetValue(HttpHeaderKeys.CorrelationId, out var value))
            {
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(headers));
            }

            if (!Guid.TryParse(value[0], out var correlationId) || correlationId == Guid.Empty)
            {
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", value);
            }

            return correlationId;
        }

        public static string GetCmsAuthValues(this HttpRequestHeaders headers)
        {
            ArgumentNullException.ThrowIfNull(headers);

            headers.TryGetValues(HttpHeaderKeys.CmsAuthValues, out var values);

            return values.First();
        }

        public static string GetCmsAuthValues(this IHeaderDictionary headers)
        {
            ArgumentNullException.ThrowIfNull(headers);

            headers.TryGetValue(HttpHeaderKeys.CmsAuthValues, out var values);

            return values.FirstOrDefault();
        }

        public static Guid EstablishCorrelation(HttpRequest req) =>
             req.Headers.TryGetValue(HttpHeaderKeys.CorrelationId, out var correlationId) &&
             Guid.TryParse(correlationId, out var parsedCorrelationId) ?
                 parsedCorrelationId :
                 Guid.NewGuid();

        public static CmsAuthValues BuildCmsAuthValues(this HttpRequest req)
        {
            if (req is null)
            {
                return null;
            }

            var cmsAuthValues = req.Headers.GetCmsAuthValues();

            if (string.IsNullOrWhiteSpace(cmsAuthValues)
                && req.Cookies.TryGetValue(HttpHeaderKeys.CmsAuthValues, out var cookieValue))
            {
                cmsAuthValues = cookieValue;
            }

            var correlation = EstablishCorrelation(req);

            return new CmsAuthValues(cmsAuthValues, correlation);
        }
    }
}