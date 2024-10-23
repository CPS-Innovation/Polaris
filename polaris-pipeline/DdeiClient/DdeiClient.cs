using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response;
using Common.Wrappers;
using Ddei.Domain.Response;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.Response.PreCharge;
using Ddei.Factories;
using Ddei.Mappers;
using Ddei.Exceptions;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Domain.Response.Defendant;
using Ddei.Domain.Response.Document;

namespace Ddei
{
    public class DdeiClient : IDdeiClient
    {
        private readonly IDdeiArgFactory _caseDataServiceArgFactory;
        private readonly ICaseDetailsMapper _caseDetailsMapper;
        private readonly ICaseDocumentMapper<DdeiDocumentResponse> _caseDocumentMapper;
        private readonly ICaseDocumentNoteMapper _caseDocumentNoteMapper;
        private readonly ICaseDocumentNoteResultMapper _caseDocumentNoteResultMapper;
        private readonly ICaseExhibitProducerMapper _caseExhibitProducerMapper;
        private readonly ICaseWitnessMapper _caseWitnessMapper;
        private readonly ICaseIdentifiersMapper _caseIdentifiersMapper;
        private readonly ICmsMaterialTypeMapper _cmsMaterialTypeMapper;
        private readonly ICaseWitnessStatementMapper _caseWitnessStatementMapper;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IDdeiClientRequestFactory _ddeiClientRequestFactory;
        private readonly ILogger<DdeiClient> _logger;
        protected readonly HttpClient _httpClient;

        public DdeiClient(
            HttpClient httpClient,
            IDdeiClientRequestFactory ddeiClientRequestFactory,
            IDdeiArgFactory caseDataServiceArgFactory,
            ICaseDetailsMapper caseDetailsMapper,
            ICaseDocumentMapper<DdeiDocumentResponse> caseDocumentMapper,
            ICaseDocumentNoteMapper caseDocumentNoteMapper,
            ICaseDocumentNoteResultMapper caseDocumentNoteResultMapper,
            ICaseExhibitProducerMapper caseExhibitProducerMapper,
            ICaseWitnessMapper caseWitnessMapper,
            ICaseIdentifiersMapper caseIdentifiersMapper,
            ICmsMaterialTypeMapper cmsMaterialTypeMapper,
            ICaseWitnessStatementMapper caseWitnessStatementMapper,
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
            _cmsMaterialTypeMapper = cmsMaterialTypeMapper ?? throw new ArgumentNullException(nameof(cmsMaterialTypeMapper));
            _caseWitnessStatementMapper = caseWitnessStatementMapper ?? throw new ArgumentNullException(nameof(caseWitnessStatementMapper));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _ddeiClientRequestFactory = ddeiClientRequestFactory ?? throw new ArgumentNullException(nameof(ddeiClientRequestFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task VerifyCmsAuthAsync(DdeiBaseArgDto arg)
        {
            // Will throw in the same way as any other call if auth is not correct.
            await CallDdei(_ddeiClientRequestFactory.CreateVerifyCmsAuthRequest(arg));
        }

        public async Task<CaseIdentifiersDto> GetUrnFromCaseIdAsync(DdeiCaseIdOnlyArgDto arg)
        {
            var result = await CallDdei<DdeiCaseIdentifiersDto>(_ddeiClientRequestFactory.CreateUrnLookupRequest(arg));
            return _caseIdentifiersMapper.MapCaseIdentifiers(result);
        }

        public async Task<IEnumerable<CaseDto>> ListCasesAsync(DdeiUrnArgDto arg)
        {
            var caseIdentifiers = await ListCaseIdsAsync(arg);

            var calls = caseIdentifiers.Select(async caseIdentifier =>
                 await GetCaseInternalAsync(_caseDataServiceArgFactory.CreateCaseArgFromUrnArg(arg, caseIdentifier.Id)));

            var cases = await Task.WhenAll(calls);
            return cases.Select(@case => _caseDetailsMapper.MapCaseDetails(@case));
        }

        public async Task<CaseDto> GetCaseAsync(DdeiCaseIdentifiersArgDto arg)
        {
            var @case = await GetCaseInternalAsync(arg);
            return _caseDetailsMapper.MapCaseDetails(@case);
        }

        public async Task<IEnumerable<PcdRequestCoreDto>> GetPcdRequestsAsync(DdeiCaseIdentifiersArgDto arg)
        {
            var pcdRequests = await CallDdei<IEnumerable<DdeiPcdRequestCoreDto>>(_ddeiClientRequestFactory.CreateGetPcdRequestsRequest(arg));
            return _caseDetailsMapper.MapCorePreChargeDecisionRequests(pcdRequests);
        }

        public async Task<PcdRequestDto> GetPcdRequestAsync(DdeiPcdArgDto arg)
        {
            var pcdRequest = await CallDdei<DdeiPcdRequestDto>(_ddeiClientRequestFactory.CreateGetPcdRequest(arg));
            return _caseDetailsMapper.MapPreChargeDecisionRequest(pcdRequest);
        }

        public async Task<DefendantsAndChargesListDto> GetDefendantAndChargesAsync(DdeiCaseIdentifiersArgDto arg)
        {
            var response = await CallDdei(_ddeiClientRequestFactory.CreateGetDefendantAndChargesRequest(arg));
            var content = await response.Content.ReadAsStringAsync();
            var defendantAndCharges = _jsonConvertWrapper.DeserializeObject<IEnumerable<DdeiCaseDefendantDto>>(content);
            var etag = response.Headers.ETag?.Tag;

            return _caseDetailsMapper.MapDefendantsAndCharges(defendantAndCharges, arg.CaseId, etag);
        }

        public async Task<IEnumerable<CmsDocumentDto>> ListDocumentsAsync(DdeiCaseIdentifiersArgDto arg)
        {
            var ddeiResults = await CallDdei<List<DdeiDocumentResponse>>(
                _ddeiClientRequestFactory.CreateListCaseDocumentsRequest(arg)
            );

            return ddeiResults
                .Select(ddeiResult => _caseDocumentMapper.Map(ddeiResult));
        }

        public async Task<FileResult> GetDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg)
        {
            var response = await CallDdei(_ddeiClientRequestFactory.CreateGetDocumentRequest(arg));
            var fileName = response.Content.Headers.GetValues("Content-Disposition").ToList()[0];

            return new FileResult
            {
                Stream = await response.Content.ReadAsStreamAsync(),
                FileName = fileName
            };
        }

        public async Task<Stream> GetDocumentFromFileStoreAsync(string path, string cmsAuthValues, Guid correlationId)
        {
            var response = await CallDdei(
                _ddeiClientRequestFactory.CreateDocumentFromFileStoreRequest(new DdeiFileStoreArgDto
                {
                    Path = path,
                    CmsAuthValues = cmsAuthValues,
                    CorrelationId = correlationId
                })
            );

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<CheckoutDocumentDto> CheckoutDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg)
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

        public async Task CancelCheckoutDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg)
        {
            await CallDdei(_ddeiClientRequestFactory.CreateCancelCheckoutDocumentRequest(arg));
        }

        public async Task<HttpResponseMessage> UploadPdfAsync(DdeiDocumentIdAndVersionIdArgDto arg, Stream stream)
        {
            return await CallDdei(_ddeiClientRequestFactory.CreateUploadPdfRequest(arg, stream), new HttpStatusCode[]
            {
                HttpStatusCode.Gone,
                HttpStatusCode.RequestEntityTooLarge
            });
        }

        public async Task<IEnumerable<DocumentNoteDto>> GetDocumentNotesAsync(DdeiDocumentNotesArgDto arg)
        {
            var ddeiResults = await CallDdei<List<DdeiDocumentNoteResponse>>(_ddeiClientRequestFactory.CreateGetDocumentNotesRequest(arg));

            return ddeiResults.Select(ddeiResult => _caseDocumentNoteMapper.Map(ddeiResult)).ToArray();
        }

        public async Task<DocumentNoteResult> AddDocumentNoteAsync(DdeiAddDocumentNoteArgDto arg)
        {
            var response = await CallDdei<DdeiDocumentNoteAddedResponse>(_ddeiClientRequestFactory.CreateAddDocumentNoteRequest(arg));

            return _caseDocumentNoteResultMapper.Map(response);
        }

        public async Task<DocumentRenamedResultDto> RenameDocumentAsync(DdeiRenameDocumentArgDto arg)
        {
            var response = await CallDdei<DdeiDocumentRenamedResponse>(_ddeiClientRequestFactory.CreateRenameDocumentRequest(arg));

            return new DocumentRenamedResultDto { Id = response.Id, OperationName = response.OperationName };
        }

        public async Task<DocumentReclassifiedResultDto> ReclassifyDocumentAsync(DdeiReclassifyDocumentArgDto arg)
        {
            var response = await CallDdei<DdeiDocumentReclassifiedResponse>(_ddeiClientRequestFactory.CreateReclassifyDocumentRequest(arg));

            return new DocumentReclassifiedResultDto
            {
                DocumentId = response.Id,
                DocumentTypeId = response.DocumentTypeId,
                ReclassificationType = response.ReclassificationType,
                OriginalDocumentTypeId = response.OriginalDocumentTypeId,
                DocumentRenamed = response.DocumentRenamed,
                DocumentRenamedOperationName = response.DocumentRenamedOperationName
            };
        }

        public async Task<IEnumerable<ExhibitProducerDto>> GetExhibitProducersAsync(DdeiCaseIdentifiersArgDto arg)
        {
            var ddeiResults = await CallDdei<List<DdeiDocumentExhibitProducerResponse>>(_ddeiClientRequestFactory.CreateGetExhibitProducersRequest(arg));

            return ddeiResults.Select(ddeiResult => _caseExhibitProducerMapper.Map(ddeiResult)).ToArray();
        }

        public async Task<IEnumerable<CaseWitnessDto>> GetWitnessesAsync(DdeiCaseIdentifiersArgDto arg)
        {
            var ddeiResults = await CallDdei<List<DdeiCaseWitnessResponse>>(_ddeiClientRequestFactory.CreateCaseWitnessesRequest(arg));

            return ddeiResults.Select(ddeiResult => _caseWitnessMapper.Map(ddeiResult)).ToArray();
        }

        public async Task<IEnumerable<MaterialTypeDto>> GetMaterialTypeListAsync(DdeiBaseArgDto arg)
        {
            var ddeiResults = await CallDdei<List<DdeiMaterialTypeListResponse>>(_ddeiClientRequestFactory.CreateGetMaterialTypeListRequest(arg));

            return ddeiResults.Select(ddeiResult => _cmsMaterialTypeMapper.Map(ddeiResult)).ToArray();
        }

        public async Task<IEnumerable<WitnessStatementDto>> GetWitnessStatementsAsync(DdeiWitnessStatementsArgDto arg)
        {
            var ddeiResults = await CallDdei<List<DdeiCaseWitnessStatementsResponse>>(_ddeiClientRequestFactory.CreateGetWitnessStatementsRequest(arg));

            return ddeiResults.Select(ddeiResult => _caseWitnessStatementMapper.Map(ddeiResult)).ToArray();
        }

        private async Task<IEnumerable<DdeiCaseIdentifiersDto>> ListCaseIdsAsync(DdeiUrnArgDto arg)
        {
            return await CallDdei<IEnumerable<DdeiCaseIdentifiersDto>>(_ddeiClientRequestFactory.CreateListCasesRequest(arg));
        }

        private async Task<DdeiCaseDetailsDto> GetCaseInternalAsync(DdeiCaseIdentifiersArgDto arg)
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