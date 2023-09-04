using Common.Configuration;
using Common.Dto.Tracker;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using polaris_integration.tests;

namespace polaris_gateway.integration.tests.Proxies
{
    public class CoordinatorApiProxy : BaseFunctionTest
    {
        protected async Task CaseDeleteAsync(string caseUrn, int caseId)
        {
            string url = MakeUrl(RestApi.Case, new Dictionary<string, string>() { { "caseUrn", caseUrn }, { "caseId", $"{caseId}" } });

            var response = await MakeHttpCall(url, HttpMethod.Delete);
            if (response.IsSuccessStatusCode && await response.Content.ReadAsStringAsync() == string.Empty)
                return;

            foreach (var i in Fibonacci(10))
            {
                await Task.Delay(i * 250);

                response = await MakeHttpCall(url, HttpMethod.Delete);
                if (response.IsSuccessStatusCode && await response.Content.ReadAsStringAsync() == string.Empty)
                    return;
            }

            return;
        }

        protected async Task CaseRefreshAsync(string caseUrn, int caseId, string correlationId)
        {
            string url = MakeUrl(RestApi.Case, new Dictionary<string, string>() { { "caseUrn", caseUrn }, { "caseId", $"{caseId}" } });
            var response = await MakeHttpCall(url, HttpMethod.Post, correlationId);

            if (response.StatusCode == HttpStatusCode.Accepted)
                return;

            throw new Exception($"Case refresh failed with status code {response.StatusCode}");
        }

        protected async Task<TrackerDto> WaitForCompletedTrackerAsync(string urn, int caseId, string correlationId)
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


        protected async Task<TrackerDto> GetTrackerAsync(string caseUrn, int caseId, string correlationId)
        {
            string url = MakeUrl(RestApi.CaseTracker, new Dictionary<string, string>() { { "caseUrn", caseUrn }, { "caseId", $"{caseId}" } });
            var tracker = await SendJsonRequestAsync<TrackerDto>(url, HttpMethod.Get, correlationId);

            return tracker;
        }

        protected async Task<HttpResponseMessage> MakeHttpCall(string url, HttpMethod httpMethod, string correlationId = null, object payload = null)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(httpMethod, $"{_polarisCoordinatorUrl}api/{url}?code={_polarisCoordinatorCode}");
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
