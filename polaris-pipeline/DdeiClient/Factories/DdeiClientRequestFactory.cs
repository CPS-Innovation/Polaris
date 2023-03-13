using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net;
using Ddei.Factories.Contracts;
using Common.Constants;
using Ddei.Domain.CaseData.Args;
using Ddei.Options;

namespace Ddei.Factories
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

        public HttpRequestMessage CreateCmsModernTokenRequest(CmsCaseDataArg arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/cms-modern-token");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateListCasesRequest(CmsUrnArg arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateGetCaseRequest(CmsCaseArg arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateListCaseDocumentsRequest(CmsCaseArg arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateCheckoutDocumentRequest(CmsDocumentArg arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.CmsDocCategory}/{arg.DocumentId}/checkout");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateCancelCheckoutDocumentRequest(CmsDocumentArg arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.CmsDocCategory}/{arg.DocumentId}/checkout");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateUploadPdfRequest(CmsDocumentArg arg, Stream stream, string filename)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.CmsDocCategory}/{arg.DocumentId}/{filename}");
            AddAuthHeaders(request, arg);
            request.Content = new StreamContent(stream);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            return request;
        }

        private void AddAuthHeaders(HttpRequestMessage request, CmsCaseDataArg arg)
        {
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