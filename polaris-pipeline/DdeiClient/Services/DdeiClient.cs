using Ddei.Factories.Contracts;
using Ddei.Exceptions;
using Domain.Exceptions;
using Ddei.Domain.CaseData.Args;
using Common.Dto.Case;
using DdeiClient.Services.Contracts;
using DdeiClient.Mappers.Contract;
using Common.Wrappers.Contracts;
using Ddei.Domain;
using Common.Domain.Extensions;
using Common.Dto.Document;
using Common.Dto.Response;
using Common.Logging;
using Common.Mappers.Contracts;
using Ddei.Options;
using Microsoft.Extensions.Logging;
using Common.Factories.Contracts;
using Microsoft.Extensions.Options;
using Common.Exceptions;

namespace Ddei.Services
{
    public class DdeiClient : IDdeiClient
    {
        private readonly DdeiOptions _ddeiOptions;
        private readonly ICaseDataArgFactory _caseDataServiceArgFactory;
        private readonly ICaseDetailsMapper _caseDetailsMapper;
        private readonly ICaseDocumentMapper<DdeiCaseDocumentResponse> _caseDocumentMapper;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IDdeiClientRequestFactory _ddeiClientRequestFactory;
        private readonly ILogger<DdeiClient> _logger;
        protected readonly IHttpRequestFactory _httpRequestFactory;
        protected readonly HttpClient _httpClient;

        static readonly string ListDocumentsUrlFormat = "api/urns/{0}/cases/{1}/documents?code={2}";
        static readonly string GetDocumentUrlFormat = "api/urns/{0}/cases/{1}/documents/{2}/{3}?code={4}";

        public DdeiClient(
            HttpClient httpClient,
            IHttpRequestFactory httpRequestFactory,
            IOptions<DdeiOptions> ddeiOptions,
            ICaseDataArgFactory caseDataServiceArgFactory,
            ICaseDetailsMapper caseDetailsMapper,
            ICaseDocumentMapper<DdeiCaseDocumentResponse> caseDocumentMapper,
            IJsonConvertWrapper jsonConvertWrapper,
            IDdeiClientRequestFactory ddeiClientRequestFactory,
            ILogger<DdeiClient> logger
            )
        {
            _ddeiOptions = ddeiOptions?.Value ?? throw new ArgumentNullException(nameof(ddeiOptions));
            _caseDataServiceArgFactory = caseDataServiceArgFactory ?? throw new ArgumentNullException(nameof(caseDataServiceArgFactory));
            _caseDetailsMapper = caseDetailsMapper ?? throw new ArgumentNullException(nameof(caseDetailsMapper));
            _caseDocumentMapper = caseDocumentMapper ?? throw new ArgumentNullException(nameof(caseDocumentMapper));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _ddeiClientRequestFactory = ddeiClientRequestFactory ?? throw new ArgumentNullException(nameof(ddeiClientRequestFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpRequestFactory = httpRequestFactory ?? throw new ArgumentNullException(nameof(httpRequestFactory));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> GetCmsModernToken(DdeiCmsCaseDataArgDto arg)
        {
            try
            {
                return await CallDdei<string>
                (
                    () => _ddeiClientRequestFactory.CreateCmsModernTokenRequest(arg), arg.CorrelationId
                );
            }
            catch (Exception exception)
            {
                throw new CaseDataServiceException("Exception in GetCmsModernToken", exception);
            }
        }

        private async Task<IEnumerable<DdeiCaseIdentifiersDto>> ListCaseIdsAsync(DdeiCmsUrnArgDto arg)
        {
            return await CallDdei<IEnumerable<DdeiCaseIdentifiersDto>>(
                () => _ddeiClientRequestFactory.CreateListCasesRequest(arg),
                 arg.CorrelationId
            );
        }

        public async Task<DdeiCaseDetailsDto> GetCaseAsync(DdeiCmsCaseArgDto arg)
        {
            return await CallDdei<DdeiCaseDetailsDto>(
                () => _ddeiClientRequestFactory.CreateGetCaseRequest(arg),
                arg.CorrelationId
            );
        }

        public async Task<IEnumerable<CaseDto>> ListCases(DdeiCmsUrnArgDto arg)
        {
            try
            {
                var caseIdentifiers = await ListCaseIdsAsync(arg);

                var calls = caseIdentifiers.Select(async caseIdentifier =>
                     await GetCaseAsync(_caseDataServiceArgFactory.CreateCaseArgFromUrnArg(arg, caseIdentifier.Id)));

                var cases = await Task.WhenAll(calls);

                return cases.Select(@case => _caseDetailsMapper.MapCaseDetails(@case));
            }
            catch (Exception exception)
            {
                throw new CaseDataServiceException("Exception in ListCases", exception);
            }
        }

        public async Task<CaseDto> GetCase(DdeiCmsCaseArgDto arg)
        {
            try
            {
                var @case = await GetCaseAsync(arg);
                return _caseDetailsMapper.MapCaseDetails(@case);
            }
            catch (Exception exception)
            {
                throw new CaseDataServiceException("Exception in GetCase", exception);
            }
        }

        public async Task<DocumentDto[]> ListDocumentsAsync(string caseUrn, string caseId, string cmsAuthValues, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(ListDocumentsAsync), $"CaseUrn: {caseUrn}, CaseId: {caseId}");
            var results = new List<DocumentDto>();

            string requestUri = string.Format(ListDocumentsUrlFormat, caseUrn, caseId, _ddeiOptions.AccessKey);
            var response = await GetHttpContentAsync(requestUri, cmsAuthValues, correlationId);
            var stringContent = await response.ReadAsStringAsync();
            var ddeiResults = _jsonConvertWrapper.DeserializeObject<List<DdeiCaseDocumentResponse>>(stringContent);

            if (ddeiResults != null && ddeiResults.Any())
                results = ddeiResults.Select(ddeiResult => _caseDocumentMapper.Map(ddeiResult)).ToList();

            _logger.LogMethodExit(correlationId, nameof(ListDocumentsAsync), results.ToJson());

            return results.ToArray();
        }

        public async Task<Stream> GetDocumentAsync(string caseUrn, string caseId, string documentCategory, string documentId, string cmsAuthValues, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetDocumentAsync), $"CaseUrn: {caseUrn}, CaseId: {caseId}, DocumentId: {documentId}");

            string requestUri = string.Format(GetDocumentUrlFormat, caseUrn, caseId, documentCategory, documentId, _ddeiOptions.AccessKey);
            var content = await GetHttpContentAsync(requestUri, cmsAuthValues, correlationId);
            var result = await content.ReadAsStreamAsync();

            _logger.LogMethodExit(correlationId, nameof(GetDocumentAsync), string.Empty);
            return result;
        }

        public async Task CheckoutDocument(DdeiCmsDocumentArgDto arg)
        {
            try
            {
                await CallDdei
                (
                   () => _ddeiClientRequestFactory.CreateCheckoutDocumentRequest(arg), arg.CorrelationId
                );
            }
            catch (Exception exception)
            {
                throw new DocumentServiceException("Exception in CheckoutDocument", exception);
            }
        }

        public async Task CancelCheckoutDocument(DdeiCmsDocumentArgDto arg)
        {
            try
            {
                await CallDdei
                (
                   () => _ddeiClientRequestFactory.CreateCancelCheckoutDocumentRequest(arg), arg.CorrelationId
                );
            }
            catch (Exception exception)
            {
                throw new DocumentServiceException("Exception in CheckoutDocument", exception);
            }
        }

        public async Task UploadPdf(DdeiCmsDocumentArgDto arg, Stream stream)
        {
            try
            {
                await CallDdei
                (
                   () => _ddeiClientRequestFactory.CreateUploadPdfRequest(arg, stream), arg.CorrelationId
                );
            }
            catch (Exception exception)
            {
                throw new DocumentServiceException("Exception in UploadPdf", exception);
            }
        }

        private async Task<T> CallDdei<T>(Func<HttpRequestMessage> requestFactory, Guid correlationId)
        {
            using var response = await CallDdei(requestFactory, correlationId);

            var content = await response.Content.ReadAsStringAsync();
            return _jsonConvertWrapper.DeserializeObject<T>(content);
        }

        private async Task<HttpResponseMessage> CallDdei(Func<HttpRequestMessage> requestFactory, Guid correlationId)
        {
            var request = requestFactory();
            var response = await _httpClient.SendAsync(request);
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(content);
                }
                return response;
            }
            catch (HttpRequestException exception)
            {
                throw new DdeiClientException(response.StatusCode, exception);
            }
        }

        private async Task<HttpContent> GetHttpContentAsync(string requestUri, string cmsAuthValues, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetHttpContentAsync), $"RequestUri: {requestUri}");

            var request = _httpRequestFactory.CreateGet(requestUri, cmsAuthValues, correlationId);
            _logger.LogMethodFlow(correlationId, nameof(GetHttpContentAsync), $"{request.Method} {_httpClient.BaseAddress}{requestUri}");

            var response = await _httpClient.SendAsync(request);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException exception)
            {
                throw new HttpException(response.StatusCode, exception);
            }

            var result = response.Content;
            _logger.LogMethodExit(correlationId, nameof(GetHttpContentAsync), string.Empty);
            return result;
        }
    }
}