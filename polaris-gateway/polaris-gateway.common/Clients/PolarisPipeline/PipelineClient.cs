using System.Net;
using System.Net.Http;
using System.Text;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Requests;
using Common.Domain.Responses;
using Common.Domain.SearchIndex;
using Common.Factories.Contracts;
using Common.Logging;
using Common.Wrappers.Contracts;
using Gateway.Clients.PolarisPipeline.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PolarisGateway.Domain.PolarisPipeline;

namespace Gateway.Clients.PolarisPipeline
{
    public class PipelineClient : IPipelineClient
    {
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ILogger<PipelineClient> _logger;

        public PipelineClient(
            IPipelineClientRequestFactory pipelineClientRequestFactory,
            HttpClient httpClient,
            IConfiguration configuration,
            IJsonConvertWrapper jsonConvertWrapper,
            ILogger<PipelineClient> logger)
        {
            _pipelineClientRequestFactory = pipelineClientRequestFactory;
            _httpClient = httpClient;
            _configuration = configuration;
            _jsonConvertWrapper = jsonConvertWrapper;
            _logger = logger;
        }

        public async Task TriggerCoordinatorAsync(string caseUrn, int caseId, string cmsAuthValues, bool force, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(TriggerCoordinatorAsync), $"CaseId: {caseId}, Force?: {force}");
            var forceQuery = force ? "&&force=true" : string.Empty;
            _logger.LogMethodExit(correlationId, nameof(TriggerCoordinatorAsync), string.Empty);
            var url = $"{RestApi.GetCaseUrl(caseUrn, caseId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}{forceQuery}";
            await SendAuthenticatedGetRequestAsync(url, cmsAuthValues, correlationId);
        }

        public async Task<Tracker> GetTrackerAsync(string caseUrn, int caseId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetTrackerAsync), $"Acquiring the tracker for caseId {caseId}");

            HttpResponseMessage response;
            try
            {
                var url = $"{RestApi.GetCaseUrl(caseUrn, caseId)}/tracker?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                response = await SendRequestAsync(HttpMethod.Get, url, correlationId);
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }

            var stringContent = await response.Content.ReadAsStringAsync();

            _logger.LogMethodExit(correlationId, nameof(GetTrackerAsync), $"Tracker details: {stringContent}");
            return _jsonConvertWrapper.DeserializeObject<Tracker>(stringContent, correlationId);
        }

        public async Task<Stream> GetDocumentAsync(string caseUrn, int caseId, Guid polarisDocumentId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetTrackerAsync), $"Acquiring the PDF with Polaris Document Id {polarisDocumentId} for urn {caseUrn} and caseId {caseId}");

            HttpResponseMessage response;
            try
            {
                var url = $"{RestApi.GetDocumentUrl(caseUrn, caseId, polarisDocumentId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                response = await SendRequestAsync(HttpMethod.Get, url, correlationId);
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }

            var streamContent = await response.Content.ReadAsStreamAsync();

            return streamContent;
        }

        public async Task<string> GenerateDocumentSasUrlAsync(string caseUrn, int caseId, Guid polarisDocumentId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetTrackerAsync), $"Generating PDF SAS Url for Polaris Document Id {polarisDocumentId}, urn {caseUrn} and caseId {caseId}");

            HttpResponseMessage response;
            try
            {
                var url = $"{RestApi.GetDocumentUrl(caseUrn, caseId, polarisDocumentId)}/sasUrl?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                response = await SendRequestAsync(HttpMethod.Get, url, correlationId);
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }

            var sasUrl = await response.Content.ReadAsStringAsync();

            return sasUrl;
        }

        public async Task<IActionResult> CheckoutDocumentAsync(string caseUrn, int caseId, Guid polarisDocumentId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(CheckoutDocumentAsync), $"Checking out the Polaris Document, with Id {polarisDocumentId} for urn {caseUrn} and caseId {caseId}");

            HttpResponseMessage response;
            try
            {
                var url = $"{RestApi.GetDocumentUrl(caseUrn, caseId, polarisDocumentId)}/checkout?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                response = await SendRequestAsync(HttpMethod.Post, url, correlationId);
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }

            return new OkResult();
        }

        public async Task<IActionResult> CancelCheckoutDocumentAsync(string caseUrn, int caseId, Guid polarisDocumentId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetTrackerAsync), $"Cancelling Checkout of the Polaris Document, with Id {polarisDocumentId} for urn {caseUrn} and caseId {caseId}");

            HttpResponseMessage response;
            try
            {
                var url = $"{RestApi.GetDocumentUrl(caseUrn, caseId, polarisDocumentId)}/checkout?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                response = await SendRequestAsync(HttpMethod.Delete, url, correlationId);
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }

            var streamContent = await response.Content.ReadAsStreamAsync();

            return new OkResult();
        }

        public async Task<RedactPdfResponse> SaveRedactionsAsync(string caseUrn, int caseId, Guid polarisDocumentId, RedactPdfRequest redactPdfRequest, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetTrackerAsync), $"Saving Redactions for Polaris Document, with Id {polarisDocumentId} for urn {caseUrn} and caseId {caseId}");

            HttpResponseMessage response;
            try
            {
                var content = new StringContent(_jsonConvertWrapper.SerializeObject(redactPdfRequest, correlationId), Encoding.UTF8, "application/json");
                var url = $"{RestApi.GetDocumentUrl(caseUrn, caseId, polarisDocumentId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                response = await SendRequestAsync(HttpMethod.Put, url, correlationId, content);
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }

            var stringContent = await response.Content.ReadAsStringAsync();

            _logger.LogMethodExit(correlationId, nameof(SaveRedactionsAsync), stringContent);
            return _jsonConvertWrapper.DeserializeObject<RedactPdfResponse>(stringContent, correlationId);
        }

        public async Task<IList<StreamlinedSearchLine>> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(SearchCase), $"Searching case {caseUrn} / {caseId} for search Term '{searchTerm}'");

            HttpResponseMessage response;
            try
            {
                var url = $"{RestApi.GetDocumentsUrl(caseUrn, caseId)}/search/{searchTerm}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                response = await SendRequestAsync(HttpMethod.Get, url, correlationId);
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }

            var stringContent = await response.Content.ReadAsStringAsync();

            _logger.LogMethodExit(correlationId, nameof(SearchCase), $"Search Result: {stringContent}");

            return _jsonConvertWrapper.DeserializeObject<IList<StreamlinedSearchLine>>(stringContent, correlationId);
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod httpMethod, string requestUri, Guid correlationId, HttpContent content=null)
        {
            _logger.LogMethodEntry(correlationId, nameof(SendRequestAsync), requestUri);

            var request = _pipelineClientRequestFactory.Create(httpMethod, requestUri, correlationId);
            request.Content = content;
            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            _logger.LogMethodExit(correlationId, nameof(SendRequestAsync), string.Empty);
            return response;
        }

        private async Task<HttpResponseMessage> SendAuthenticatedGetRequestAsync(string requestUri, string cmsAuthValues, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(SendAuthenticatedGetRequestAsync), requestUri);

            var request = _pipelineClientRequestFactory.CreateAuthenticatedGet(requestUri, cmsAuthValues, correlationId);
            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            _logger.LogMethodExit(correlationId, nameof(SendAuthenticatedGetRequestAsync), string.Empty);
            return response;
        }
    }
}

