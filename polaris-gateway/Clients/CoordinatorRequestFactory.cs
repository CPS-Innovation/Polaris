using Common.Constants;
using Microsoft.Extensions.Configuration;

namespace PolarisGateway.Clients
{
    public class CoordinatorRequestFactory : ICoordinatorRequestFactory
    {
        private readonly IConfiguration _configuration;
        public CoordinatorRequestFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues, HttpContent content)
        {
            var request = new HttpRequestMessage(httpMethod, requestUri);
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
            if (cmsAuthValues != null)
            {
                request.Headers.Add(HttpHeaderKeys.CmsAuthValues, cmsAuthValues);
            }
            request.Content = content;
            request.Headers.Add("x-functions-key", _configuration[ConfigurationKeys.PipelineCoordinatorFunctionAppKey]);
            return request;
        }
    }
}

