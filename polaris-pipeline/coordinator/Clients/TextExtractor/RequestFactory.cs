using System;
using System.Net.Http;
using Common.Constants;

namespace coordinator.Clients.TextExtractor
{
    public class RequestFactory : IRequestFactory
    {
        public HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId)
        {
            var request = new HttpRequestMessage(httpMethod, requestUri);
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());

            return request;
        }
    }
}