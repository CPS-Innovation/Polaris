using System.Net;
using System.Text;
using Common.Configuration;
using Common.Constants;
using Common.Domain.SearchIndex;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Dto.Tracker;
using Common.Factories.Contracts;
using Common.Logging;
using Common.Streaming;
using Common.ValueObjects;
using Common.Wrappers.Contracts;
using Gateway.Clients.PolarisPipeline.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gateway.Clients.PolarisPipeline
{
    public class PipelineClient : IPipelineClient
    {
        private readonly IPipelineClientRequestFactory _pipelineClientRequestFactory;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IHttpResponseMessageStreamFactory _httpResponseMessageStreamFactory;
        private readonly ILogger<PipelineClient> _logger;

        private static readonly HttpStatusCode[] ExpectedRefreshErrorStatusCodes = { HttpStatusCode.Locked };
        private static readonly HttpStatusCode[] ExpectedCheckoutErrorStatusCodes = { HttpStatusCode.Conflict };

        public PipelineClient(
            IPipelineClientRequestFactory pipelineClientRequestFactory,
            HttpClient httpClient,
            IConfiguration configuration,
            IJsonConvertWrapper jsonConvertWrapper,
            IHttpResponseMessageStreamFactory httpResponseMessageStreamFactory,
            ILogger<PipelineClient> logger)
        {
            _pipelineClientRequestFactory = pipelineClientRequestFactory;
            _httpClient = httpClient;
            _configuration = configuration;
            _jsonConvertWrapper = jsonConvertWrapper;
            _httpResponseMessageStreamFactory = httpResponseMessageStreamFactory;
            _logger = logger;
        }

        public async Task<StatusCodeResult> RefreshCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(RefreshCaseAsync), $"CaseId: {caseId}");

            var url = $"{RestApi.GetCasePath(caseUrn, caseId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
            var response = await SendRequestAsync(HttpMethod.Post, url, cmsAuthValues, correlationId, null, ExpectedRefreshErrorStatusCodes);
            HttpStatusCode statusCode = response.StatusCode;

            _logger.LogMethodExit(correlationId, nameof(RefreshCaseAsync), statusCode.ToString());

            return new StatusCodeResult((int)statusCode);
        }

        public async Task<IActionResult> DeleteCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(DeleteCaseAsync), $"CaseId: {caseId}");

            var url = $"{RestApi.GetCasePath(caseUrn, caseId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
            var response = await SendRequestAsync(HttpMethod.Delete, url, cmsAuthValues, correlationId, null, ExpectedRefreshErrorStatusCodes);
            HttpStatusCode statusCode = response.StatusCode;

            _logger.LogMethodExit(correlationId, nameof(RefreshCaseAsync), statusCode.ToString());

            return new StatusCodeResult((int)statusCode);
        }

        public async Task<TrackerDto> GetTrackerAsync(string caseUrn, int caseId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetTrackerAsync), $"Acquiring the tracker for caseId {caseId}");

            HttpResponseMessage response;
            try
            {
                var url = $"{RestApi.GetCaseTrackerPath(caseUrn, caseId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                response = await SendRequestAsync(HttpMethod.Get, url, null, correlationId);
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
            return _jsonConvertWrapper.DeserializeObject<TrackerDto>(stringContent, correlationId);
        }

        public async Task<Stream> GetDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetTrackerAsync), $"Acquiring the PDF with Polaris Document Id {polarisDocumentId} for urn {caseUrn} and caseId {caseId}");

            HttpResponseMessage response;
            try
            {
                var url = $"{RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                response = await SendRequestAsync(HttpMethod.Get, url, null, correlationId);
                return await _httpResponseMessageStreamFactory.Create(response);
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    // todo: check that returning null here is good logic
                    return null;
                }

                throw;
            }
        }

        public async Task<string> GenerateDocumentSasUrlAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetTrackerAsync), $"Generating PDF SAS Url for Polaris Document Id {polarisDocumentId}, urn {caseUrn} and caseId {caseId}");

            HttpResponseMessage response;
            try
            {
                var url = $"{RestApi.GetDocumentSasPath(caseUrn, caseId, polarisDocumentId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                response = await SendRequestAsync(HttpMethod.Get, url, null, correlationId);
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

        public async Task<IActionResult> CheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(CheckoutDocumentAsync), $"Checking out the Polaris Document, with Id {polarisDocumentId} for urn {caseUrn} and caseId {caseId}");

            HttpResponseMessage response;
            try
            {
                var url = $"{RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId)}/checkout?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                response = await SendRequestAsync(HttpMethod.Post, url, cmsAuthValues, correlationId, null, ExpectedCheckoutErrorStatusCodes);
            }
            catch (HttpRequestException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                throw;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return new OkResult();

                case HttpStatusCode.Conflict:
                    var lockingUser = await response.Content.ReadAsStringAsync();
                    return new ConflictObjectResult(lockingUser);

                default:
                    return new StatusCodeResult(500);
            };
        }

        public async Task<IActionResult> CancelCheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetTrackerAsync), $"Cancelling Checkout of the Polaris Document, with Id {polarisDocumentId} for urn {caseUrn} and caseId {caseId}");

            try
            {
                var url = $"{RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId)}/checkout?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
                await SendRequestAsync(HttpMethod.Delete, url, cmsAuthValues, correlationId);
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

        public async Task<RedactPdfResponse> SaveRedactionsAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetTrackerAsync), $"Saving Redactions for Polaris Document, with Id {polarisDocumentId} for urn {caseUrn} and caseId {caseId}");

            var content = new StringContent(_jsonConvertWrapper.SerializeObject(redactPdfRequest, correlationId), Encoding.UTF8, "application/json");
            var url = $"{RestApi.GetDocumentPath(caseUrn, caseId, polarisDocumentId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}";
            var response = await SendRequestAsync(HttpMethod.Put, url, cmsAuthValues, correlationId, content);
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
                var url = $"{RestApi.GetCaseSearchPath(caseUrn, caseId)}?code={_configuration[PipelineSettings.PipelineCoordinatorFunctionAppKey]}&query={searchTerm}";
                response = await SendRequestAsync(HttpMethod.Get, url, null, correlationId);
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

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod httpMethod, string requestUri, string cmsAuthValues, Guid correlationId, HttpContent content = null, HttpStatusCode[] expectedResponseCodes = null)
        {
            _logger.LogMethodEntry(correlationId, nameof(SendRequestAsync), requestUri);

            var request = _pipelineClientRequestFactory.Create(httpMethod, requestUri, correlationId, cmsAuthValues);
            request.Content = content;

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (expectedResponseCodes?.Contains(response.StatusCode) != true)
                response.EnsureSuccessStatusCode();

            _logger.LogMethodExit(correlationId, nameof(SendRequestAsync), string.Empty);
            return response;
        }
    }
}

