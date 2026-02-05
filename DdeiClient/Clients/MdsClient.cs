using Common.Dto.Response;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Extensions;
using Common.Wrappers;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Domain.Response;
using Ddei.Domain.Response.Defendant;
using Ddei.Domain.Response.Document;
using Ddei.Domain.Response.PreCharge;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Domain.Args;
using DdeiClient.Domain.Response;
using DdeiClient.Domain.Response.Document;
using DdeiClient.Factories;
using Microsoft.Extensions.Logging;
using System.Net;

namespace DdeiClient.Clients;

public class MdsClient : BaseCmsClient, IMdsClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMdsClientRequestFactory _mdsClientRequestFactory;
    private readonly IMdsArgFactory _caseDataServiceArgFactory;
    private readonly ICaseDetailsMapper _caseDetailsMapper;
    private readonly ICaseDocumentMapper<DdeiDocumentResponse> _caseDocumentMapper;
    private readonly ICaseDocumentNoteMapper _caseDocumentNoteMapper;
    private readonly ICaseDocumentNoteResultMapper _caseDocumentNoteResultMapper;
    private readonly ICaseExhibitProducerMapper _caseExhibitProducerMapper;
    private readonly ICaseIdentifiersMapper _caseIdentifiersMapper;
    private readonly ICmsMaterialTypeMapper _cmsMaterialTypeMapper;
    private readonly ICaseWitnessStatementMapper _caseWitnessStatementMapper;
    private readonly IJsonConvertWrapper _jsonConvertWrapper;
    private readonly ILogger<MdsClient> _logger;
    private readonly IMdsClientFactory _mdsClientFactory;
    public MdsClient(
        IHttpClientFactory httpClientFactory, 
        IMdsClientRequestFactory mdsClientRequestFactory, 
        IMdsArgFactory caseDataServiceArgFactory, 
        ICaseDetailsMapper caseDetailsMapper, 
        ICaseDocumentMapper<DdeiDocumentResponse> caseDocumentMapper, 
        ICaseDocumentNoteMapper caseDocumentNoteMapper, 
        ICaseDocumentNoteResultMapper caseDocumentNoteResultMapper, 
        ICaseExhibitProducerMapper caseExhibitProducerMapper, 
        ICaseIdentifiersMapper caseIdentifiersMapper, 
        ICmsMaterialTypeMapper cmsMaterialTypeMapper, 
        ICaseWitnessStatementMapper caseWitnessStatementMapper, 
        ILogger<MdsClient> logger, 
        IMdsClientFactory mdsClientFactory, 
        IJsonConvertWrapper jsonConvertWrapper) 
        : base(jsonConvertWrapper)
    {
        _httpClientFactory = httpClientFactory.ExceptionIfNull();
        _mdsClientRequestFactory = mdsClientRequestFactory.ExceptionIfNull();
        _caseDataServiceArgFactory = caseDataServiceArgFactory.ExceptionIfNull();
        _caseDetailsMapper = caseDetailsMapper.ExceptionIfNull();
        _caseDocumentMapper = caseDocumentMapper.ExceptionIfNull();
        _caseDocumentNoteMapper = caseDocumentNoteMapper.ExceptionIfNull();
        _caseDocumentNoteResultMapper = caseDocumentNoteResultMapper.ExceptionIfNull();
        _caseExhibitProducerMapper = caseExhibitProducerMapper.ExceptionIfNull();
        _caseIdentifiersMapper = caseIdentifiersMapper.ExceptionIfNull();
        _cmsMaterialTypeMapper = cmsMaterialTypeMapper.ExceptionIfNull();
        _caseWitnessStatementMapper = caseWitnessStatementMapper.ExceptionIfNull();
        _logger = logger.ExceptionIfNull();
        _mdsClientFactory = mdsClientFactory.ExceptionIfNull();
        _jsonConvertWrapper = jsonConvertWrapper.ExceptionIfNull();
    }

    public async Task<CaseIdentifiersDto> GetUrnFromCaseIdAsync(MdsCaseIdOnlyArgDto arg)
    {
        var mdsCaseIdentifiersDto = await CallHttpClientAsync<MdsCaseIdentifiersDto>(_mdsClientRequestFactory.CreateUrnLookupRequest(arg), arg.CmsAuthValues);

        return _caseIdentifiersMapper.MapCaseIdentifiers(mdsCaseIdentifiersDto);
    }

    public async Task<IEnumerable<CaseDto>> ListCasesAsync(MdsUrnArgDto arg)
    {
        var caseIdentifiers = await ListCaseIdsAsync(arg);

        var calls = caseIdentifiers.Select(async caseIdentifier =>
            await GetCaseInternalAsync(_caseDataServiceArgFactory.CreateCaseArgFromUrnArg(arg, caseIdentifier.Id)));

        var cases = await Task.WhenAll(calls);
        return cases.Select(@case => _caseDetailsMapper.MapCaseDetails(@case));
    }

    public async Task<CaseDto> GetCaseAsync(MdsCaseIdentifiersArgDto arg)
    {
        var @case = await GetCaseInternalAsync(arg);
        return _caseDetailsMapper.MapCaseDetails(@case);
    }

    public async Task<CaseSummaryDto> GetCaseSummaryAsync(MdsCaseIdOnlyArgDto arg)
    {
        var ddeiResult = await CallHttpClientAsync<MdsCaseSummaryDto>(_mdsClientRequestFactory.CreateGetCaseSummary(arg), arg.CmsAuthValues);
        return _caseDetailsMapper.Map(ddeiResult);
    }

    public async Task<IEnumerable<PcdRequestCoreDto>> GetPcdRequestsCoreAsync(MdsCaseIdentifiersArgDto arg)
    {
        var pcdRequests = await CallHttpClientAsync<IEnumerable<DdeiPcdRequestCoreDto>>(_mdsClientRequestFactory.CreateGetPcdRequestsRequest(arg), arg.CmsAuthValues);
        return _caseDetailsMapper.MapCorePreChargeDecisionRequests(pcdRequests);
    }

    public async Task<IEnumerable<PcdRequestDto>> GetPcdRequestsAsync(MdsCaseIdentifiersArgDto arg)
    {
        var pcdRequests = await CallHttpClientAsync<IEnumerable<DdeiPcdRequestDto>>(_mdsClientRequestFactory.CreateGetPcdRequestsRequest(arg), arg.CmsAuthValues);
        return _caseDetailsMapper.MapPreChargeDecisionRequests(pcdRequests);
    }

    public async Task<PcdRequestDto> GetPcdRequestAsync(MdsPcdArgDto arg)
    {
        var pcdRequest = await CallHttpClientAsync<DdeiPcdRequestDto>(_mdsClientRequestFactory.CreateGetPcdRequest(arg), arg.CmsAuthValues);
        return _caseDetailsMapper.MapPreChargeDecisionRequest(pcdRequest);
    }

    public async Task<DefendantsAndChargesListDto> GetDefendantAndChargesAsync(MdsCaseIdentifiersArgDto arg)
    {
        var response = await CallHttpClientAsync(_mdsClientRequestFactory.CreateGetDefendantAndChargesRequest(arg), arg.CmsAuthValues);
        var content = await response.Content.ReadAsStringAsync();
        var defendantAndCharges = _jsonConvertWrapper.DeserializeObject<IEnumerable<DdeiCaseDefendantDto>>(content);
        var etag = response.Headers.ETag?.Tag;

        return _caseDetailsMapper.MapDefendantsAndCharges(defendantAndCharges, arg.CaseId, etag);
    }

    public async Task<IEnumerable<CmsDocumentDto>> ListDocumentsAsync(MdsCaseIdentifiersArgDto arg)
    {
        var ddeiResults = await CallHttpClientAsync<List<DdeiDocumentResponse>>(_mdsClientRequestFactory.CreateListCaseDocumentsRequest(arg), arg.CmsAuthValues);

        return ddeiResults.Select(ddeiResult => _caseDocumentMapper.Map(ddeiResult));
    }

    public async Task<FileResult> GetDocumentAsync(MdsDocumentIdAndVersionIdArgDto arg)
    {
        var response = await CallHttpClientAsync(_mdsClientRequestFactory.CreateGetDocumentRequest(arg), arg.CmsAuthValues);
        var fileName = response.Content.Headers.GetValues("Content-Disposition").ToList()[0];

        return new FileResult
        {
            Stream = await response.Content.ReadAsStreamAsync(),
            FileName = fileName
        };
    }

    public async Task<Stream> GetDocumentFromFileStoreAsync(string path, string cmsAuthValues, Guid correlationId)
    {
        var response = await CallHttpClientAsync(
            _mdsClientRequestFactory.CreateDocumentFromFileStoreRequest(new MdsFileStoreArgDto
            {
                Path = path,
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId
            }),
            cmsAuthValues
        );

        return await response.Content.ReadAsStreamAsync();
    }

    public async Task<CheckoutDocumentDto> CheckoutDocumentAsync(MdsDocumentIdAndVersionIdArgDto arg)
    {
        var response = await CallHttpClientAsync(
            _mdsClientRequestFactory.CreateCheckoutDocumentRequest(arg), arg.CmsAuthValues, HttpStatusCode.Conflict);

        return response.StatusCode == HttpStatusCode.Conflict ?
            new CheckoutDocumentDto
            {
                IsSuccess = false,
                LockingUserName = await response.Content.ReadAsStringAsync()
            } :
            new CheckoutDocumentDto
            {
                IsSuccess = true
            };
    }

    public async Task CancelCheckoutDocumentAsync(MdsDocumentIdAndVersionIdArgDto arg)
    {
        await CallHttpClientAsync(_mdsClientRequestFactory.CreateCancelCheckoutDocumentRequest(arg), arg.CmsAuthValues);
    }

    public async Task<HttpResponseMessage> UploadPdfAsync(MdsDocumentIdAndVersionIdArgDto arg, Stream stream)
    {
        return await CallHttpClientAsync(_mdsClientRequestFactory.CreateUploadPdfRequest(arg, stream), arg.CmsAuthValues,
        [
            HttpStatusCode.Gone,
            HttpStatusCode.RequestEntityTooLarge
        ]);
    }

    public async Task<IEnumerable<DocumentNoteDto>> GetDocumentNotesAsync(MdsDocumentArgDto arg)
    {
        var ddeiResults = await CallHttpClientAsync<List<DocumentNoteResponse>>(_mdsClientRequestFactory.CreateGetDocumentNotesRequest(arg), arg.CmsAuthValues);

        return ddeiResults.Select(ddeiResult => _caseDocumentNoteMapper.Map(ddeiResult)).ToArray();
    }

    public async Task<DocumentNoteResult> AddDocumentNoteAsync(MdsAddDocumentNoteArgDto arg)
    {
        var response = await CallHttpClientAsync<DdeiDocumentNoteAddedResponse>(_mdsClientRequestFactory.CreateAddDocumentNoteRequest(arg), arg.CmsAuthValues);

        return _caseDocumentNoteResultMapper.Map(response);
    }

    public async Task<DocumentRenamedResultDto> RenameDocumentAsync(MdsRenameDocumentArgDto arg)
    {
        var response = await CallHttpClientAsync<RenameMaterialResponse>(_mdsClientRequestFactory.CreateRenameDocumentRequest(arg), arg.CmsAuthValues);

        return new DocumentRenamedResultDto { Id = response.UpdateCommunication.Id };
    }

    public async Task<DocumentRenamedResultDto> RenameExhibitAsync(MdsRenameDocumentArgDto arg)
    {
        var response = await CallHttpClientAsync<RenameMaterialDescriptionResponse>(_mdsClientRequestFactory.CreateRenameExhibitRequest(arg), arg.CmsAuthValues);

        return new DocumentRenamedResultDto { Id = response.UpdateCommunicationDescription.Id };
    }

    public async Task<DocumentReclassifiedResultDto> ReclassifyDocumentAsync(MdsReclassifyDocumentArgDto arg)
    {
        var response = await CallHttpClientAsync<DdeiDocumentReclassifiedResponse>(_mdsClientRequestFactory.CreateReclassifyDocumentRequest(arg), arg.CmsAuthValues);

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

    public async Task<MdsCommunicationReclassifiedResponse> ReclassifyCommunicationAsync(MdsReclassifyCommunicationArgDto arg)
    {
        return await CallHttpClientAsync<MdsCommunicationReclassifiedResponse>(_mdsClientRequestFactory.CreateReclassifyCommunicationRequest(arg), arg.CmsAuthValues);
    }

    public async Task<IEnumerable<ExhibitProducerDto>> GetExhibitProducersAsync(MdsCaseIdentifiersArgDto arg)
    {
        var ddeiResults = await CallHttpClientAsync<MdsDocumentExhibitProducerResponse>(_mdsClientRequestFactory.CreateGetExhibitProducersRequest(arg), arg.CmsAuthValues);

        return ddeiResults.ExhibitProducers.Select(_caseExhibitProducerMapper.Map);
    }

    public async Task<IEnumerable<BaseCaseWitnessResponse>> GetWitnessesAsync(MdsCaseIdentifiersArgDto arg)
    {
        var ddeiResults = await CallHttpClientAsync<List<MdsCaseWitnessResponse>>(_mdsClientRequestFactory.CreateCaseWitnessesRequest(arg), arg.CmsAuthValues);

        return ddeiResults;
    }

    public async Task<IEnumerable<MaterialTypeDto>> GetMaterialTypeListAsync(CmsBaseArgDto arg)
    {
        var ddeiResults = await CallHttpClientAsync<List<MdsMaterialTypeListResponse>>(_mdsClientRequestFactory.CreateGetMaterialTypeListRequest(arg), arg.CmsAuthValues);

        return ddeiResults.Select(ddeiResult => _cmsMaterialTypeMapper.Map(ddeiResult)).ToArray();
    }

    public async Task<IEnumerable<WitnessStatementDto>> GetWitnessStatementsAsync(MdsWitnessStatementsArgDto arg)
    {
        var ddeiResults = await CallHttpClientAsync<StatementsForWitnessResponse>(_mdsClientRequestFactory.CreateGetWitnessStatementsRequest(arg), arg.CmsAuthValues);

        return ddeiResults.StatementsForWitness.Select(_caseWitnessStatementMapper.Map).ToArray();
    }

    public async Task<bool> ToggleIsUnusedDocumentAsync(MdsToggleIsUnusedDocumentDto toggleIsUnusedDocumentDto) =>
        (await CallHttpClientAsync(_mdsClientRequestFactory.CreateToggleIsUnusedDocumentRequest(toggleIsUnusedDocumentDto), toggleIsUnusedDocumentDto.CmsAuthValues))
        .IsSuccessStatusCode;

    public async Task<IEnumerable<MdsCaseIdentifiersDto>> ListCaseIdsAsync(MdsUrnArgDto arg) =>
        await CallHttpClientAsync<IEnumerable<MdsCaseIdentifiersDto>>(_mdsClientRequestFactory.CreateListCasesRequest(arg), arg.CmsAuthValues);

    private async Task<DdeiCaseDetailsDto> GetCaseInternalAsync(MdsCaseIdentifiersArgDto arg) =>
        await CallHttpClientAsync<DdeiCaseDetailsDto>(_mdsClientRequestFactory.CreateGetCaseRequest(arg), arg.CmsAuthValues);

    protected override HttpClient GetHttpClient(string cmsAuthValues)
    {
        var mdsClientName = _mdsClientFactory.Create(cmsAuthValues);
        return _httpClientFactory.CreateClient(mdsClientName);
    }
}