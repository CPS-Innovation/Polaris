using System;
using System.Linq;
using System.Net.Http.Headers;
using Common.Constants;
using Common.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Common.Extensions
{
    public static class HttpRequestHeadersExtensions
    {
        public static Guid GetCorrelationId(this HttpRequestHeaders headers)
        {
            headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
            if (correlationIdValues == null)
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(headers));

            var correlationId = correlationIdValues.First();
            if (!Guid.TryParse(correlationId, out var currentCorrelationId) || currentCorrelationId == Guid.Empty)
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", correlationId);

            return currentCorrelationId;
        }

        public static Guid GetCorrelationId(this IHeaderDictionary headers)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            if (!headers.TryGetValue(HttpHeaderKeys.CorrelationId, out var value))
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(headers));

            if (!Guid.TryParse(value[0], out var correlationId) || correlationId == Guid.Empty)
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", value);

            return correlationId;
        }

        public static string GetCmsAuthValues(this HttpRequestHeaders headers)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            if (!headers.TryGetValues(HttpHeaderKeys.CmsAuthValues, out var values))
                throw new BadRequestException($"A valid {HttpHeaderKeys.CmsAuthValues} header is required.", nameof(headers));

            var value = values.First();
            if (string.IsNullOrWhiteSpace(value))
                throw new BadRequestException($"A valid {HttpHeaderKeys.CmsAuthValues} header is required.", value);

            return value;
        }

        public static string GetCmsAuthValues(this IHeaderDictionary headers)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            if (!headers.TryGetValue(HttpHeaderKeys.CmsAuthValues, out var values))
                throw new BadRequestException($"A valid {HttpHeaderKeys.CmsAuthValues} header is required.", nameof(headers));

            var value = values.First();
            if (string.IsNullOrWhiteSpace(value))
                throw new BadRequestException($"A valid {HttpHeaderKeys.CmsAuthValues} header is required.", value);

            return value;
        }
    }
}