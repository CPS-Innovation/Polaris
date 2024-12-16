using Common.Configuration;
using PolarisGateway.Clients.Coordinator;

namespace PolarisGateway.Clients.PdfThumbnailGenerator
{ 
    public class PdfThumbnailGeneratorClient : IPdfThumbnailGeneratorClient 
    {
        private readonly IRequestFactory _requestFactory;
        private readonly HttpClient _httpClient;

        public PdfThumbnailGeneratorClient(IRequestFactory requestFactory, HttpClient httpClient)
        {
            _requestFactory = requestFactory;
            _httpClient = httpClient;
        }
        
        public async Task<HttpResponseMessage> GetThumbnailAsync(string caseUrn, int caseId, string documentId, int versionId, int maxDimensionPixel, int pageIndex, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(HttpMethod.Get, RestApi.GetThumbnailPath(caseUrn, caseId, documentId, versionId, maxDimensionPixel, pageIndex), correlationId, cmsAuthValues);
        }

        public async Task<HttpResponseMessage> GenerateThumbnailAsync(string caseUrn, int caseId, string documentId, int versionId, int maxDimensionPixel, int? pageIndex, string cmsAuthValues, Guid correlationId)
        {
            return await SendRequestAsync(HttpMethod.Post, RestApi.GetThumbnailPath(caseUrn, caseId, documentId, versionId, maxDimensionPixel, pageIndex), correlationId, cmsAuthValues);
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues = null, HttpContent content = null)
        {
            var request = _requestFactory.Create(httpMethod, requestUri, correlationId, cmsAuthValues, content);
            return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        }
    }
}