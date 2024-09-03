using Common.Constants;

namespace PolarisGateway.Clients
{
    public class RequestFactory : IRequestFactory
    {
        public HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues, HttpContent content)
        {
            var request = new HttpRequestMessage(httpMethod, requestUri)
            {
                Content = content
            };
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
            request.Headers.Add(HttpHeaderKeys.CmsAuthValues, cmsAuthValues);

            return request;
        }
    }
}

