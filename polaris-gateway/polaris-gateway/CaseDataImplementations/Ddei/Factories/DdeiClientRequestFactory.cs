using System.Net.Http;
using Microsoft.Extensions.Options;
using PolarisGateway.Domain.CaseData.Args;
using PolarisGateway.CaseDataImplementations.Ddei.Options;
using System.IO;
using System.Net.Http.Headers;
using System.Net;

namespace PolarisGateway.CaseDataImplementations.Ddei.Factories
{
    public class DdeiClientRequestFactory : IDdeiClientRequestFactory
    {
        private const string CorrelationId = "Correlation-Id";
        private const string FunctionKey = "x-functions-key";

        private readonly DdeiOptions _options;

        public DdeiClientRequestFactory(IOptions<DdeiOptions> ddeiOptions)
        {
            _options = ddeiOptions.Value;
        }

        public HttpRequestMessage CreateCmsModernTokenRequest(CaseDataArg arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/cms-modern-token");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateListCasesRequest(UrnArg arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateGetCaseRequest(CaseArg arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateListCaseDocumentsRequest(CaseArg arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateCheckoutDocumentRequest(DocumentArg arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.CmsDocCategory}/{arg.DocumentId}/checkout");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateCancelCheckoutDocumentRequest(DocumentArg arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.CmsDocCategory}/{arg.DocumentId}/checkout");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateUploadPdfRequest(DocumentArg arg, Stream stream, string filename)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.CmsDocCategory}/{arg.DocumentId}/{filename}");
            AddAuthHeaders(request, arg);
            request.Content = new StreamContent(stream);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            return request;
        }

        private void AddAuthHeaders(HttpRequestMessage request, CaseDataArg arg)
        {
            request.Headers.Add(AuthenticationKeys.Authorization, $"{AuthenticationKeys.Bearer} {arg.OnBehalfOfToken}");
            request.Headers.Add(HttpHeaderKeys.CmsAuthValues, arg.CmsAuthValues);
            if (!string.IsNullOrEmpty(_options.AccessKey))
            {
                request.Headers.Add(FunctionKey, _options.AccessKey);
            }
            request.Headers.Add(CorrelationId, arg.CorrelationId.ToString());
        }

        private string Encode(string param) => WebUtility.UrlEncode(param);
    }
}