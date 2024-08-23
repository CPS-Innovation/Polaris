using System.Net;
using Microsoft.Extensions.Logging;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Wrappers;
using Ddei.Domain;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.PreCharge;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Exceptions;
using DdeiClient.Mappers;
using DdeiClient.Services;

namespace Ddei.Services
{
    public class DdeiClient : IDdeiClient
    {
        private readonly IDdeiArgFactory _caseDataServiceArgFactory;
        private readonly ICaseDetailsMapper _caseDetailsMapper;
        private readonly ICaseDocumentMapper<DdeiCaseDocumentResponse> _caseDocumentMapper;
        private readonly ICaseDocumentNoteMapper _caseDocumentNoteMapper;
        private readonly ICaseDocumentNoteResultMapper _caseDocumentNoteResultMapper;
        private readonly ICaseExhibitProducerMapper _caseExhibitProducerMapper;
        private readonly ICaseWitnessMapper _caseWitnessMapper;
        private readonly ICaseIdentifiersMapper _caseIdentifiersMapper;
        private readonly ICmsAuthValuesMapper _cmsAuthValuesMapper;
        private readonly ICmsMaterialTypeMapper _cmsMaterialTypeMapper;
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
            ICaseExhibitProducerMapper caseExhibitProducerMapper,
            ICaseWitnessMapper caseWitnessMapper,
            ICaseIdentifiersMapper caseIdentifiersMapper,
            ICmsAuthValuesMapper cmsAuthValuesMapper,
            ICmsMaterialTypeMapper cmsMaterialTypeMapper,
            IJsonConvertWrapper jsonConvertWrapper,
            ILogger<DdeiClient> logger
            )
        {
            _caseDataServiceArgFactory = caseDataServiceArgFactory ?? throw new ArgumentNullException(nameof(caseDataServiceArgFactory));
            _caseDetailsMapper = caseDetailsMapper ?? throw new ArgumentNullException(nameof(caseDetailsMapper));
            _caseDocumentMapper = caseDocumentMapper ?? throw new ArgumentNullException(nameof(caseDocumentMapper));
            _caseDocumentNoteMapper = caseDocumentNoteMapper ?? throw new ArgumentNullException(nameof(caseDocumentNoteMapper));
            _caseDocumentNoteResultMapper = caseDocumentNoteResultMapper ?? throw new ArgumentNullException(nameof(caseDocumentNoteResultMapper));
            _caseExhibitProducerMapper = caseExhibitProducerMapper ?? throw new ArgumentNullException(nameof(caseExhibitProducerMapper));
            _caseWitnessMapper = caseWitnessMapper ?? throw new ArgumentNullException(nameof(caseWitnessMapper));
            _caseIdentifiersMapper = caseIdentifiersMapper ?? throw new ArgumentNullException(nameof(caseIdentifiersMapper));
            _cmsAuthValuesMapper = cmsAuthValuesMapper ?? throw new ArgumentNullException(nameof(cmsAuthValuesMapper));
            _cmsMaterialTypeMapper = cmsMaterialTypeMapper ?? throw new ArgumentNullException(nameof(cmsMaterialTypeMapper));
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

        public async Task<IEnumerable<ExhibitProducerDto>> GetExhibitProducers(DdeiCmsCaseArgDto arg)
        {
            var ddeiResults = await CallDdei<List<DdeiCaseDocumentExhibitProducerResponse>>(_ddeiClientRequestFactory.CreateGetExhibitProducersRequest(arg));

            return ddeiResults.Select(ddeiResult => _caseExhibitProducerMapper.Map(ddeiResult)).ToArray();
        }

        public async Task<IEnumerable<CaseWitnessDto>> GetWitnesses(DdeiCmsCaseArgDto arg)
        {
            var ddeiResults = await CallDdei<List<DdeiCaseWitnessResponse>>(_ddeiClientRequestFactory.CreateCaseWitnessesRequest(arg));

            return ddeiResults.Select(ddeiResult => _caseWitnessMapper.Map(ddeiResult)).ToArray();
        }

        public async Task<IEnumerable<MaterialTypeDto>> GetMaterialTypeListAsync(DdeiCmsCaseDataArgDto arg)
        {
            var ddeiResults = await CallDdei<List<DdeiMaterialTypeListResponse>>(_ddeiClientRequestFactory.CreateGetMaterialTypeListRequest(arg));

            return ddeiResults.Select(ddeiResult => _cmsMaterialTypeMapper.Map(ddeiResult)).ToArray();
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