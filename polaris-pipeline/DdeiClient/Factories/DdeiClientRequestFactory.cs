using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Common.Constants;
using Common.Dto.Request;
using Ddei.Domain.CaseData.Args;

namespace Ddei.Factories.Contracts
{
    public class DdeiClientRequestFactory : IDdeiClientRequestFactory
    {
        private const string CorrelationId = "Correlation-Id";

        public HttpRequestMessage CreateCmsAuthValuesRequest(DdeiCmsCaseDataArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/cms-auth-values");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateUrnLookupRequest(DdeiCmsCaseIdArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urn-lookup/{arg.CaseId}");
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
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.DocumentId}/{arg.VersionId}/checkout");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateCancelCheckoutDocumentRequest(DdeiCmsDocumentArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.DocumentId}/{arg.VersionId}/checkout");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateDocumentFromFileStoreRequest(DdeiCmsFileStoreArgDto arg)
        {
            // note that `arg.Path` is already prefixed with a slash
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/file-store{arg.Path}");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateUploadPdfRequest(DdeiCmsDocumentArgDto arg, Stream stream)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.DocumentId}/{arg.VersionId}");
            AddAuthHeaders(request, arg);
            request.Content = new StreamContent(stream);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            return request;
        }

        public HttpRequestMessage CreateStatusRequest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/status");
            return request;
        }

        public HttpRequestMessage CreateGetDocumentNotesRequest(DdeiCmsDocumentNotesArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.DocumentCategory}/{arg.DocumentId}/notes");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateAddDocumentNoteRequest(DdeiCmsAddDocumentNoteArgDto arg)
        {
            var content = JsonSerializer.Serialize(new AddDocumentNoteDto
            {
                DocumentId = arg.DocumentId,
                Text = arg.Text
            });
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.DocumentId}/notes");
            AddAuthHeaders(request, arg);
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");
            return request;
        }

        private void AddAuthHeaders(HttpRequestMessage request, DdeiCmsCaseDataArgDto arg)
        {
            request.Headers.Add(HttpHeaderKeys.CmsAuthValues, arg.CmsAuthValues);
            request.Headers.Add(CorrelationId, arg.CorrelationId.ToString());
        }

        private string Encode(string param) => WebUtility.UrlEncode(param);
    }
}