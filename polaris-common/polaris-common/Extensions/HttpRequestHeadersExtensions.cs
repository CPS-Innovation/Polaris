using Common.Constants;
using Common.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace polaris_common.Extensions
{
    public static class HttpRequestHeadersExtensions
    {
        public static Guid GetCorrelationId(this HttpRequest request)
        {
            var correlationIdFound = request.Headers.TryGetValue(HttpHeaderKeys.CorrelationId, out var correlationIdValue);
            if (!correlationIdFound || !Guid.TryParse(correlationIdValue.FirstOrDefault(), out var currentCorrelationId) || currentCorrelationId == Guid.Empty)
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.",
                    nameof(request));

            return currentCorrelationId;
        }
    }
}