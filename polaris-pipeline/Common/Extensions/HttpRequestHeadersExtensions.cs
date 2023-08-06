using System;
using System.Linq;
using System.Net.Http.Headers;
using Common.Constants;
using Common.Domain.Exceptions;

namespace Common.Extensions
{
    public static class HttpRequestHeadersExtensions
    {
        public static Guid GetCorrelationId(this HttpRequestHeaders headers)
        {
            Guid currentCorrelationId = default;

            headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
            if (correlationIdValues == null)
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(headers));

            var correlationId = correlationIdValues.First();
            if (!Guid.TryParse(correlationId, out currentCorrelationId) || currentCorrelationId == Guid.Empty)
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.",
                    correlationId);

            return currentCorrelationId;
        }
    }
}