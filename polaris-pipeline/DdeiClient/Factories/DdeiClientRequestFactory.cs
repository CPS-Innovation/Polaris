using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Common.Constants;
using Common.Dto.Request;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;

namespace Ddei.Factories.Contracts
{
    public class DdeiClientRequestFactory : IDdeiClientRequestFactory
    {
        private const string CorrelationId = "Correlation-Id";

        public HttpRequestMessage CreateVerifyCmsAuthRequest(DdeiBaseArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/verify-cms-auth");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateUrnLookupRequest(DdeiCaseIdOnlyArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urn-lookup/{arg.CaseId}");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateListCasesRequest(DdeiUrnArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateGetCaseRequest(DdeiCaseIdentifiersArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateGetPcdRequestsRequest(DdeiCaseIdentifiersArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/pcd-requests");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateGetPcdRequest(DdeiPcdArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/pcd-requests/{arg.PcdId}");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateGetDefendantAndChargesRequest(DdeiCaseIdentifiersArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/defendants");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateListCaseDocumentsRequest(DdeiCaseIdentifiersArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateCheckoutDocumentRequest(DdeiDocumentIdAndVersionIdArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.DocumentId}/versions/{arg.VersionId}/checkout");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateCancelCheckoutDocumentRequest(DdeiDocumentIdAndVersionIdArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.DocumentId}/versions/{arg.VersionId}/checkout");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateGetDocumentRequest(DdeiDocumentIdAndVersionIdArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.DocumentId}/versions/{arg.VersionId}");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateDocumentFromFileStoreRequest(DdeiFileStoreArgDto arg)
        {
            // note that `arg.Path` is already prefixed with a slash
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/file-store{arg.Path}");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateUploadPdfRequest(DdeiDocumentIdAndVersionIdArgDto arg, Stream stream)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.DocumentId}/versions/{arg.VersionId}");
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

        public HttpRequestMessage CreateGetDocumentNotesRequest(DdeiDocumentArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.DocumentId}/notes");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateAddDocumentNoteRequest(DdeiAddDocumentNoteArgDto arg)
        {
            var content = JsonSerializer.Serialize(new AddDocumentNoteDto
            {
                Text = arg.Text
            });
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.DocumentId}/notes");
            AddAuthHeaders(request, arg);
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");
            return request;
        }

        public HttpRequestMessage CreateRenameDocumentRequest(DdeiRenameDocumentArgDto arg)
        {
            var content = JsonSerializer.Serialize(new RenameDocumentDto
            {
                DocumentName = arg.DocumentName
            });
            var request = new HttpRequestMessage(HttpMethod.Put, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.DocumentId}/rename");
            AddAuthHeaders(request, arg);
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");
            return request;
        }

        public HttpRequestMessage CreateReclassifyDocumentRequest(DdeiReclassifyDocumentArgDto arg)
        {
            var content = JsonSerializer.Serialize(new ReclassifyDocumentDto
            {
                DocumentTypeId = arg.DocumentTypeId,
                Exhibit = arg.Exhibit,
                Statement = arg.Statement,
                Other = arg.Other,
                Immediate = arg.Immediate
            });
            var request = new HttpRequestMessage(HttpMethod.Put, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/documents/{arg.DocumentId}/reclassify");
            AddAuthHeaders(request, arg);
            request.Content = new StringContent(content, Encoding.UTF8, ContentType.Json);
            return request;
        }

        public HttpRequestMessage CreateGetExhibitProducersRequest(DdeiCaseIdentifiersArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/exhibit-producers");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateCaseWitnessesRequest(DdeiCaseIdentifiersArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/witnesses");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateGetMaterialTypeListRequest(DdeiBaseArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/reference/reclassification");
            AddAuthHeaders(request, arg);
            return request;
        }

        public HttpRequestMessage CreateGetWitnessStatementsRequest(DdeiWitnessStatementsArgDto arg)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases/{arg.CaseId}/witnesses/{arg.WitnessId}/statements");
            AddAuthHeaders(request, arg);
            return request;
        }

        private void AddAuthHeaders(HttpRequestMessage request, DdeiBaseArgDto arg)
        {
            request.Headers.Add(HttpHeaderKeys.CmsAuthValues, arg.CmsAuthValues);
            request.Headers.Add(CorrelationId, arg.CorrelationId.ToString());
        }

        private string Encode(string param) => WebUtility.UrlEncode(param);
    }
}