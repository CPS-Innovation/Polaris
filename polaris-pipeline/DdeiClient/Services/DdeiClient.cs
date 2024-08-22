using System.Net;
using Common.Dto.Case;
using Common.Dto.Document;
using Common.Dto.Response;
using Common.Wrappers;
using Ddei.Domain;
using Ddei.Domain.CaseData.Args;
using Ddei.Factories;
using DdeiClient.Exceptions;
using DdeiClient.Services;
using DdeiClient.Mappers;
using Microsoft.Extensions.Logging;
using Ddei.Mappers;
using Common.Dto.Case.PreCharge;
using Ddei.Domain.PreCharge;

namespace Ddei.Services
{
    public class DdeiClient : IDdeiClient
    {
        private readonly IDdeiArgFactory _caseDataServiceArgFactory;
        private readonly ICaseDetailsMapper _caseDetailsMapper;
        private readonly ICaseDocumentMapper<DdeiCaseDocumentResponse> _caseDocumentMapper;
        private readonly ICaseDocumentNoteMapper _caseDocumentNoteMapper;
        private readonly ICaseDocumentNoteResultMapper _caseDocumentNoteResultMapper;
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
            ICaseDocumentNoteMapper caseDocumentNoteMapper,
            ICaseDocumentNoteResultMapper caseDocumentNoteResultMapper,
            ICaseIdentifiersMapper caseIdentifiersMapper,
            ICmsAuthValuesMapper cmsAuthValuesMapper,
            IJsonConvertWrapper jsonConvertWrapper,
            ILogger<DdeiClient> logger
            )
        {
            _caseDataServiceArgFactory = caseDataServiceArgFactory ?? throw new ArgumentNullException(nameof(caseDataServiceArgFactory));
            _caseDetailsMapper = caseDetailsMapper ?? throw new ArgumentNullException(nameof(caseDetailsMapper));
            _caseDocumentMapper = caseDocumentMapper ?? throw new ArgumentNullException(nameof(caseDocumentMapper));
            _caseDocumentNoteMapper = caseDocumentNoteMapper ?? throw new ArgumentNullException(nameof(caseDocumentNoteMapper));
            _caseDocumentNoteResultMapper = caseDocumentNoteResultMapper ?? throw new ArgumentNullException(nameof(caseDocumentNoteResultMapper));
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

        public async Task<IEnumerable<PcdRequestCoreDto>> GetPcdRequests(DdeiCmsCaseArgDto arg)
        {
            var pcdRequests = await CallDdei<IEnumerable<DdeiPcdRequestCoreDto>>(_ddeiClientRequestFactory.CreateGetPcdRequestsRequest(arg));
            return _caseDetailsMapper.MapCorePreChargeDecisionRequests(pcdRequests);
        }

        public async Task<PcdRequestDto> GetPcdRequest(DdeiCmsPcdArgDto arg)
        {
            var pcdRequest = await CallDdei<DdeiPcdRequestDto>(_ddeiClientRequestFactory.CreateGetPcdRequest(arg));
            return _caseDetailsMapper.MapPreChargeDecisionRequest(pcdRequest);
        }

        public async Task<IEnumerable<DefendantAndChargesDto>> GetDefendantAndCharges(DdeiCmsCaseArgDto arg)
        {
            var defendantAndCharges = await CallDdei<IEnumerable<DdeiCaseDefendantDto>>(_ddeiClientRequestFactory.CreateGetDefendantAndChargesRequest(arg));
            return _caseDetailsMapper.MapDefendantsAndCharges(defendantAndCharges);
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

        public async Task<HttpResponseMessage> UploadPdfAsync(DdeiCmsDocumentArgDto arg, Stream stream)
        {
            return await CallDdei(_ddeiClientRequestFactory.CreateUploadPdfRequest(arg, stream), new HttpStatusCode[]
            {
                HttpStatusCode.Gone,
                HttpStatusCode.RequestEntityTooLarge
            });
        }

        public async Task<IEnumerable<DocumentNoteDto>> GetDocumentNotes(DdeiCmsDocumentNotesArgDto arg)
        {
            var ddeiResults = await CallDdei<List<DdeiCaseDocumentNoteResponse>>(_ddeiClientRequestFactory.CreateGetDocumentNotesRequest(arg));

            return ddeiResults.Select(ddeiResult => _caseDocumentNoteMapper.Map(ddeiResult)).ToArray();
        }

        public async Task<DocumentNoteResult> AddDocumentNote(DdeiCmsAddDocumentNoteArgDto arg)
        {
            var response = await CallDdei<DdeiCaseDocumentNoteAddedResponse>(_ddeiClientRequestFactory.CreateAddDocumentNoteRequest(arg));

            return _caseDocumentNoteResultMapper.Map(response);
        }

        public async Task<DocumentRenamedResult> RenameDocumentAsync(DdeiCmsRenameDocumentArgDto arg)
        {
            var response = await CallDdei<DdeiCaseDocumentRenamedResponse>(_ddeiClientRequestFactory.CreateRenameDocumentRequest(arg));

            return new DocumentRenamedResult { Id = response.Id, OperationName = response.OperationName };
        }

        public async Task<DocumentReclassifiedResult> ReclassifyDocumentAsync(DdeiCmsReclassifyDocumentArgDto arg)
        {
            var response = await CallDdei<DdeiCaseDocumentReclassifiedResponse>(_ddeiClientRequestFactory.CreateReclassifyDocumentRequest(arg));

            return new DocumentReclassifiedResult
            {
                DocumentId = response.Id,
                DocumentTypeId = response.DocumentTypeId,
                ReclassificationType = response.ReclassificationType,
                OriginalDocumentTypeId = response.OriginalDocumentTypeId,
                DocumentRenamed = response.DocumentRenamed,
                DocumentRenamedOperationName = response.DocumentRenamedOperationName
            };
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