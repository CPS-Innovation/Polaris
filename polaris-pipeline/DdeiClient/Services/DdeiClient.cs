using Ddei.Domain.CaseData.Args;
using Common.Dto.Case;
using DdeiClient.Services;
using DdeiClient.Mappers;
using Common.Wrappers.Contracts;
using Ddei.Domain;
using Common.Dto.Document;
using Common.Dto.Response;
using Microsoft.Extensions.Logging;
using Common.Exceptions;
using System.Net;
using Ddei.Factories;

namespace Ddei.Services
{
    public class DdeiClient : IDdeiClient
    {
        private readonly IDdeiArgFactory _caseDataServiceArgFactory;
        private readonly ICaseDetailsMapper _caseDetailsMapper;
        private readonly ICaseDocumentMapper<DdeiCaseDocumentResponse> _caseDocumentMapper;
        private readonly ICaseIdentifiersMapper _caseIdentifiersMapper;
        private readonly ICmsAuthValuesMapper _cmsAuthValuesMapper;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IDdeiClientRequestFactory _ddeiClientRequestFactory;
        private readonly ILogger<DdeiClient> _logger;
        protected readonly HttpClient _httpClient;

        public DdeiClient(
            HttpClient httpClient,
            IDdeiClientRequestFactory ddeiClientRequestFactory,
            IDdeiArgFactory caseDataServiceArgFactory,
            ICaseDetailsMapper caseDetailsMapper,
            ICaseDocumentMapper<DdeiCaseDocumentResponse> caseDocumentMapper,
            ICaseIdentifiersMapper caseIdentifiersMapper,
            ICmsAuthValuesMapper cmsAuthValuesMapper,
            IJsonConvertWrapper jsonConvertWrapper,
            ILogger<DdeiClient> logger
            )
        {
            _caseDataServiceArgFactory = caseDataServiceArgFactory ?? throw new ArgumentNullException(nameof(caseDataServiceArgFactory));
            _caseDetailsMapper = caseDetailsMapper ?? throw new ArgumentNullException(nameof(caseDetailsMapper));
            _caseDocumentMapper = caseDocumentMapper ?? throw new ArgumentNullException(nameof(caseDocumentMapper));
            _caseIdentifiersMapper = caseIdentifiersMapper ?? throw new ArgumentNullException(nameof(caseIdentifiersMapper));
            _cmsAuthValuesMapper = cmsAuthValuesMapper ?? throw new ArgumentNullException(nameof(cmsAuthValuesMapper));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _ddeiClientRequestFactory = ddeiClientRequestFactory ?? throw new ArgumentNullException(nameof(ddeiClientRequestFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<CmsAuthValuesDto> GetFullCmsAuthValuesAsync(DdeiCmsCaseDataArgDto arg)
        {
            var result = await CallDdei<DdeiCmsAuthValuesDto>(_ddeiClientRequestFactory.CreateCmsAuthValuesRequest(arg));
            return _cmsAuthValuesMapper.MapCmsAuthValues(result);
        }

        public async Task<CaseIdentifiersDto> GetUrnFromCaseIdAsync(DdeiCmsCaseIdArgDto arg)
        {
            var result = await CallDdei<DdeiCaseIdentifiersDto>(_ddeiClientRequestFactory.CreateUrnLookupRequest(arg));
            return _caseIdentifiersMapper.MapCaseIdentifiers(result);
        }

        private async Task<IEnumerable<DdeiCaseIdentifiersDto>> ListCaseIdsAsync(DdeiCmsUrnArgDto arg)
        {
            return await CallDdei<IEnumerable<DdeiCaseIdentifiersDto>>(_ddeiClientRequestFactory.CreateListCasesRequest(arg));
        }

        public async Task<IEnumerable<CaseDto>> ListCasesAsync(DdeiCmsUrnArgDto arg)
        {
            var caseIdentifiers = await ListCaseIdsAsync(arg);

            var calls = caseIdentifiers.Select(async caseIdentifier =>
                 await GetCaseInternalAsync(_caseDataServiceArgFactory.CreateCaseArgFromUrnArg(arg, caseIdentifier.Id)));

            var cases = await Task.WhenAll(calls);
            return cases.Select(@case => _caseDetailsMapper.MapCaseDetails(@case));
        }

        public async Task<CaseDto> GetCaseAsync(DdeiCmsCaseArgDto arg)
        {
            var @case = await GetCaseInternalAsync(arg);
            return _caseDetailsMapper.MapCaseDetails(@case);
        }

        public async Task<CmsDocumentDto[]> ListDocumentsAsync(string caseUrn, string caseId, string cmsAuthValues, Guid correlationId)
        {
            var caseArg = new DdeiCmsCaseArgDto
            {
                Urn = caseUrn,
                CaseId = long.Parse(caseId),
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId
            }; ;
            var ddeiResults = await CallDdei<List<DdeiCaseDocumentResponse>>(
                _ddeiClientRequestFactory.CreateListCaseDocumentsRequest(caseArg)
            );

            return ddeiResults
                .Select(ddeiResult => _caseDocumentMapper.Map(ddeiResult))
                .ToArray();
        }

        public async Task<Stream> GetDocumentAsync(string caseUrn, string caseId, string documentCategory, string documentId, string cmsAuthValues, Guid correlationId)
        {
            var response = await CallDdei(
                _ddeiClientRequestFactory.CreateDocumentRequest(new DdeiCmsDocumentArgDto
                {
                    Urn = caseUrn,
                    CaseId = long.Parse(caseId),
                    CmsDocCategory = documentCategory,
                    DocumentId = int.Parse(documentId),
                    CmsAuthValues = cmsAuthValues,
                    CorrelationId = correlationId
                })
            );

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<Stream> GetDocumentFromFileStoreAsync(string path, string cmsAuthValues, Guid correlationId)
        {
            var response = await CallDdei(
                _ddeiClientRequestFactory.CreateDocumentFromFileStoreRequest(new DdeiCmsFileStoreArgDto
                {
                    Path = path,
                    CmsAuthValues = cmsAuthValues,
                    CorrelationId = correlationId
                })
            );

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<CheckoutDocumentDto> CheckoutDocumentAsync(DdeiCmsDocumentArgDto arg)
        {
            var response = await CallDdei(
                _ddeiClientRequestFactory.CreateCheckoutDocumentRequest(arg),
                HttpStatusCode.Conflict);

            return response.StatusCode == HttpStatusCode.Conflict
                ? new CheckoutDocumentDto
                {
                    IsSuccess = false,
                    LockingUserName = await response.Content.ReadAsStringAsync()
                }
                : new CheckoutDocumentDto
                {
                    IsSuccess = true
                };
        }

        public async Task CancelCheckoutDocumentAsync(DdeiCmsDocumentArgDto arg)
        {
            await CallDdei(_ddeiClientRequestFactory.CreateCancelCheckoutDocumentRequest(arg));
        }

        public async Task UploadPdfAsync(DdeiCmsDocumentArgDto arg, Stream stream)
        {
            await CallDdei(_ddeiClientRequestFactory.CreateUploadPdfRequest(arg, stream));
        }

        private async Task<DdeiCaseDetailsDto> GetCaseInternalAsync(DdeiCmsCaseArgDto arg)
        {
            return await CallDdei<DdeiCaseDetailsDto>(_ddeiClientRequestFactory.CreateGetCaseRequest(arg));
        }

        private async Task<T> CallDdei<T>(HttpRequestMessage request)
        {
            using var response = await CallDdei(request);
            var content = await response.Content.ReadAsStringAsync();
            return _jsonConvertWrapper.DeserializeObject<T>(content);
        }

        private async Task<HttpResponseMessage> CallDdei(HttpRequestMessage request, params HttpStatusCode[] expectedUnhappyStatusCodes)
        {
            var response = await _httpClient.SendAsync(request);
            try
            {
                if (response.IsSuccessStatusCode || expectedUnhappyStatusCodes.Contains(response.StatusCode))
                    return response;

                var content = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(content);
            }
            catch (HttpRequestException exception)
            {
                throw new DdeiClientException(response.StatusCode, exception);
            }
        }
    }
}