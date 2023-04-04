using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net;
using Common.Constants;
using Ddei.Domain.CaseData.Args;
using Ddei.Options;

namespace Ddei.Factories.Contracts
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

        public HttpRequestMessage CreateCmsModernTokenRequest(DdeiCmsCaseDataArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/cms-modern-token");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateListCasesRequest(DdeiCmsUrnArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateGetCaseRequest(DdeiCmsCaseArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateListCaseDocumentsRequest(DdeiCmsCaseArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateCheckoutDocumentRequest(DdeiCmsDocumentArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.CmsDocCategory}/{arg.DocumentId}/{arg.VersionId}/checkout?code={_options.AccessKey}");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateCancelCheckoutDocumentRequest(DdeiCmsDocumentArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.CmsDocCategory}/{arg.DocumentId}/{arg.VersionId}/checkout?code={_options.AccessKey}");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateUploadPdfRequest(DdeiCmsDocumentArgDto arg, Stream stream)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.CmsDocCategory}/{arg.DocumentId}/{arg.VersionId}?code={_options.AccessKey}");
            AddAuthHeaders(request, arg);
            request.Content = new StreamContent(stream);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            return request;
        }

        public HttpRequestMessage CreateGet(string requestUri, string cmsAuthValues, Guid correlationId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            request.Headers.Add(HttpHeaderKeys.CmsAuthValues, cmsAuthValues);
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());

            return request;
        }

        private void AddAuthHeaders(HttpRequestMessage request, DdeiCmsCaseDataArgDto arg)
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