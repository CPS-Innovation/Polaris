using Common.Constants;
using Microsoft.Extensions.Configuration;

namespace PolarisGateway.Clients.Coordinator
{
    public class RequestFactory : IRequestFactory
    {
        private readonly IConfiguration _configuration;
        public RequestFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues, HttpContent content)
        {
            var request = new HttpRequestMessage(httpMethod, requestUri)
            {
                Content = content
            };
            request.Headers.Add(HttpHeaderKeys.FunctionsKey, _configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]);
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
            request.Headers.Add(HttpHeaderKeys.CmsAuthValues, cmsAuthValues);

            return request;
        }
    }
}

