using Common.Constants;
using Common.Factories.Contracts;
using System;
using System.Net.Http;

namespace Common.Factories
{
    public class PipelineClientRequestFactory : IPipelineClientRequestFactory
    {
        public HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues)
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

