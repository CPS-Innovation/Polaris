using polaris_common.Constants;
using polaris_common.Factories.Contracts;

namespace polaris_common.Factories
{
    public class PipelineClientRequestFactory : IPipelineClientRequestFactory
    {
        public HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues = null)
        {
            var request = new HttpRequestMessage(httpMethod, requestUri);
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
            if (cmsAuthValues != null)
            {
                request.Headers.Add(HttpHeaderKeys.CmsAuthValues, cmsAuthValues);
            }

            return request;
        }
    }
}

