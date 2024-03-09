using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Common.Constants;

namespace coordinator.Clients.TextExtractor
{
    public class RequestFactory : IRequestFactory
    {
        private readonly IConfiguration _configuration;

        public RequestFactory(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId)
        {
            var request = new HttpRequestMessage(httpMethod, requestUri);
            request.Headers.Add(HttpHeaderKeys.FunctionsKey, _configuration[Constants.ConfigKeys.PipelineTextExtractorFunctionAppKey]);
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());

            return request;
        }
    }
}