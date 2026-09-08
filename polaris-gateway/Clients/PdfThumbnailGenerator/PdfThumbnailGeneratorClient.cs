using Common.Configuration;
using Common.Helpers;
using PolarisGateway.Clients.Coordinator;
using System;
using System.Net.Http;
using System.Threading.Tasks;

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

        public async Task<HttpResponseMessage> GetThumbnailAsync(string caseUrn, int caseId, string materialId, int documentId, int maxDimensionPixel, int pageIndex, string cmsAuthValues, Guid correlationId, bool isLegacy = true)
        {
            LegacyCaseValidation.EnsureCaseUrnProvided(caseUrn, isLegacy);
            var path = isLegacy
                ? RestApi.GetThumbnailPathLegacy(caseUrn, caseId, materialId, documentId, maxDimensionPixel, pageIndex)
                : RestApi.GetThumbnailPath(caseId, materialId, documentId, maxDimensionPixel, pageIndex);

            return await SendRequestAsync(HttpMethod.Get, path, correlationId, cmsAuthValues);
        }

        public async Task<HttpResponseMessage> GenerateThumbnailAsync(string caseUrn, int caseId, string materialId, int documentId, int maxDimensionPixel, int? pageIndex, string cmsAuthValues, Guid correlationId, bool isLegacy = true)
        {
            LegacyCaseValidation.EnsureCaseUrnProvided(caseUrn, isLegacy);
            var path = isLegacy
                ? RestApi.GetThumbnailPathLegacy(caseUrn, caseId, materialId, documentId, maxDimensionPixel, pageIndex ?? 0)
                : RestApi.GetThumbnailPath(caseId, materialId, documentId, maxDimensionPixel, pageIndex ?? 0);

            return await SendRequestAsync(HttpMethod.Post, path, correlationId, cmsAuthValues);
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues = null, HttpContent content = null)
        {
            var request = _requestFactory.Create(httpMethod, requestUri, correlationId, cmsAuthValues, content);
            return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        }
    }
}
