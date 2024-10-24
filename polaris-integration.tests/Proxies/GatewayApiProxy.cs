using Common.Configuration;
using Common.Dto.Response.Case;
using Common.Dto.Response.Documents;
using System.Net;
using Newtonsoft.Json;
using Common.Domain.SearchIndex;
using Common.Dto.Request;
using System.Text;
using polaris_integration.tests;
using FluentAssertions.Equivalency;

namespace polaris_gateway.integration.tests.Proxies
{
    public class GatewayApiProxy : BaseFunctionTest
    {
        protected async Task<TrackerDto> GetTrackerAsync(string caseUrn, int caseId, string correlationId)
        {
            string url = MakeUrl(RestApi.CaseTracker, new Dictionary<string, string>() { { "caseUrn", caseUrn }, { "caseId", $"{caseId}" } });
            var tracker = await SendJsonRequestAsync<TrackerDto>(url, HttpMethod.Get, correlationId);

            return tracker;
        }

        protected async Task<string> GetTrackerTextOnlyAsync(string caseUrn, int caseId)
        {
            string url = MakeUrl(RestApi.CaseTracker, new Dictionary<string, string>() { { "caseUrn", caseUrn }, { "caseId", $"{caseId}" } });
            var trackerText = await SendJsonRequestAsync<string>(url, HttpMethod.Get);

            return trackerText;
        }

        protected async Task CaseDelete(string caseUrn, int caseId)
        {
            string url = MakeUrl(RestApi.Case, new Dictionary<string, string>() { { "caseUrn", caseUrn }, { "caseId", $"{caseId}" } });

            var response = await MakeHttpCall(url, HttpMethod.Delete);
            if (response.IsSuccessStatusCode && await response.Content.ReadAsStringAsync() == string.Empty)
                return;

            foreach (var i in Fibonacci(10))
            {
                await Task.Delay(i * 250);

                response = await MakeHttpCall(url, HttpMethod.Delete);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    if (content == string.Empty)
                        return;
                }
            }

            return;
        }

        protected async Task<CaseDto[]> GetCasesAsync(string caseUrn)
        {
            string url = MakeUrl(RestApi.Cases, new Dictionary<string, string>() { { "caseUrn", caseUrn } });
            var cases = await SendJsonRequestAsync<CaseDto[]>(url, HttpMethod.Get);

            return cases;
        }

        protected async Task<CaseDto> GetCaseAsync(string caseUrn, int caseId)
        {
            string url = MakeUrl(RestApi.Case, new Dictionary<string, string>() { { "caseUrn", caseUrn }, { "caseId", $"{caseId}" } });
            var @case = await SendJsonRequestAsync<CaseDto>(url, HttpMethod.Get);

            return @case;
        }

        protected async Task<HttpStatusCode> CaseRefresh(string caseUrn, int caseId, string correlationId)
        {
            string url = MakeUrl(RestApi.Case, new Dictionary<string, string>() { { "caseUrn", caseUrn }, { "caseId", $"{caseId}" } });
            var response = await MakeHttpCall(url, HttpMethod.Post, correlationId);

            return response.StatusCode;
        }

        protected async Task<TrackerDto> WaitForCompletedTracker(string urn, int caseId, string correlationId)
        {
            TrackerDto tracker;

            do
            {
                tracker = await GetTrackerAsync(urn, caseId, correlationId);

                if (tracker.Status == CaseRefreshStatus.Failed)
                    throw new Exception("Case refresh failed");
            }
            while (tracker.Status != CaseRefreshStatus.Completed);

            return tracker;
        }

        protected async Task<TrackerDto> GetCaseTracker(string caseUrn, int caseId)
        {
            string url = MakeUrl(RestApi.CaseTracker, new Dictionary<string, string>() { { "caseUrn", caseUrn }, { "caseId", $"{caseId}" } });

            HttpStatusCode statusCode = HttpStatusCode.NotFound;

            foreach (var i in Fibonacci(10))
            {
                var response = await MakeHttpCall(url, HttpMethod.Get);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<TrackerDto>(responseContent);
                }

                statusCode = response.StatusCode;
            }

            throw new Exception($"Case tracker failed with status code {statusCode}");
        }

        protected async Task<StreamlinedSearchLine[]> CaseSearch(string caseUrn, int caseId, string correlationId, string query)
        {
            var url = MakeUrl(RestApi.CaseSearch, new Dictionary<string, string>() { { "caseUrn", caseUrn }, { "caseId", $"{caseId}" } }) + $"?query={query}";
            var searchResults = await SendJsonRequestAsync<StreamlinedSearchLine[]>(url, HttpMethod.Get, correlationId);
            return searchResults;
        }

        protected async Task DocumentCheckout(string caseUrn, int caseId, string documentId, string correlationId)
        {
            string url = MakeUrl(RestApi.DocumentCheckout, new Dictionary<string, string>()
                            {
                                { "caseUrn", caseUrn },
                                { "caseId", $"{caseId}" },
                                { "documentId", $"{documentId}" }
                            });

            var response = await MakeHttpCall(url, HttpMethod.Post, correlationId);
            response.EnsureSuccessStatusCode();
        }

        protected async Task RedactDocument(string caseUrn, int caseId, string documentId, DocumentRedactionSaveRequestDto redaction, string correlationId)
        {
            string url = MakeUrl(RestApi.Document, new Dictionary<string, string>()
                            {
                                { "caseUrn", caseUrn },
                                { "caseId", $"{caseId}" },
                                { "documentId", $"{documentId}" }
                            });

            var response = await MakeHttpCall(url, HttpMethod.Put, correlationId, redaction);
            response.EnsureSuccessStatusCode();
        }

        protected async Task<HttpResponseMessage> MakeHttpCall(string url, HttpMethod httpMethod, string correlationId = null, object payload = null)
        {
            using var client = new HttpClient();
            var separator = url.Contains("?") ? "&" : "?";
            url = $"{_polarisGatewayUrl}api/{url}{separator}code={_polarisGatewayCode}";
            var request = new HttpRequestMessage(httpMethod, url);
            AddAuthAndContextHeaders(request, correlationId ?? Guid.NewGuid().ToString());
            if (payload != null)
            {
                var content = JsonConvert.SerializeObject(payload, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                request.Content = new StringContent(content, Encoding.UTF8, "application/json");
            }
            var response = await client.SendAsync(request);

            return response;
        }

        protected async Task<string> SendRequestAsync(string url, HttpMethod httpMethod, string correlationId = null)
        {
            var response = await MakeHttpCall(url, httpMethod, correlationId);
            var responseContent = await response.Content.ReadAsStringAsync();

            return responseContent;
        }

        protected async Task<T> SendJsonRequestAsync<T>(string url, HttpMethod httpMethod, string correlationId = null)
        {
            var json = await SendRequestAsync(url, httpMethod, correlationId);
            var dto = JsonConvert.DeserializeObject<T>(json);

            return dto;
        }
    }
}
