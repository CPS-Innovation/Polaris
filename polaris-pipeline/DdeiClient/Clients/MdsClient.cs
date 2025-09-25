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
    private readonly IDdeiArgFactory _caseDataServiceArgFactory;
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
        IDdeiArgFactory caseDataServiceArgFactory, 
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

    public async Task<CaseIdentifiersDto> GetUrnFromCaseIdAsync(DdeiCaseIdOnlyArgDto arg)
    {
        var ddeiCaseIdentifiersDto = await CallDdeiAsync<DdeiCaseIdentifiersDto>(_mdsClientRequestFactory.CreateUrnLookupRequest(arg), arg.CmsAuthValues);

        return _caseIdentifiersMapper.MapCaseIdentifiers(ddeiCaseIdentifiersDto);
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

    public async Task<CaseSummaryDto> GetCaseSummaryAsync(DdeiCaseIdOnlyArgDto arg)
    {
        var ddeiResult = await CallDdeiAsync<MdsCaseSummaryDto>(_mdsClientRequestFactory.CreateGetCaseSummary(arg), arg.CmsAuthValues);
        return _caseDetailsMapper.Map(ddeiResult);
    }

    public async Task<IEnumerable<PcdRequestCoreDto>> GetPcdRequestsCoreAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var pcdRequests = await CallDdeiAsync<IEnumerable<DdeiPcdRequestCoreDto>>(_mdsClientRequestFactory.CreateGetPcdRequestsRequest(arg), arg.CmsAuthValues);
        return _caseDetailsMapper.MapCorePreChargeDecisionRequests(pcdRequests);
    }

    public async Task<IEnumerable<PcdRequestDto>> GetPcdRequestsAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var pcdRequests = await CallDdeiAsync<IEnumerable<DdeiPcdRequestDto>>(_mdsClientRequestFactory.CreateGetPcdRequestsRequest(arg), arg.CmsAuthValues);
        return _caseDetailsMapper.MapPreChargeDecisionRequests(pcdRequests);
    }

    public async Task<PcdRequestDto> GetPcdRequestAsync(DdeiPcdArgDto arg)
    {
        var pcdRequest = await CallDdeiAsync<DdeiPcdRequestDto>(_mdsClientRequestFactory.CreateGetPcdRequest(arg), arg.CmsAuthValues);
        return _caseDetailsMapper.MapPreChargeDecisionRequest(pcdRequest);
    }

    public async Task<DefendantsAndChargesListDto> GetDefendantAndChargesAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var response = await CallDdeiAsync(_mdsClientRequestFactory.CreateGetDefendantAndChargesRequest(arg), arg.CmsAuthValues);
        var content = await response.Content.ReadAsStringAsync();
        var defendantAndCharges = _jsonConvertWrapper.DeserializeObject<IEnumerable<DdeiCaseDefendantDto>>(content);
        var etag = response.Headers.ETag?.Tag;

        return _caseDetailsMapper.MapDefendantsAndCharges(defendantAndCharges, arg.CaseId, etag);
    }

    public async Task<IEnumerable<CmsDocumentDto>> ListDocumentsAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var ddeiResults = await CallDdeiAsync<List<DdeiDocumentResponse>>(_mdsClientRequestFactory.CreateListCaseDocumentsRequest(arg), arg.CmsAuthValues);

        return ddeiResults.Select(ddeiResult => _caseDocumentMapper.Map(ddeiResult));
    }

    public async Task<FileResult> GetDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        var response = await CallDdeiAsync(_mdsClientRequestFactory.CreateGetDocumentRequest(arg), arg.CmsAuthValues);
        var fileName = response.Content.Headers.GetValues("Content-Disposition").ToList()[0];

        return new FileResult
        {
            Stream = await response.Content.ReadAsStreamAsync(),
            FileName = fileName
        };
    }

    public async Task<Stream> GetDocumentFromFileStoreAsync(string path, string cmsAuthValues, Guid correlationId)
    {
        var response = await CallDdeiAsync(
            _mdsClientRequestFactory.CreateDocumentFromFileStoreRequest(new DdeiFileStoreArgDto
            {
                Path = path,
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId
            }),
            cmsAuthValues
        );

        return await response.Content.ReadAsStreamAsync();
    }

    public async Task<CheckoutDocumentDto> CheckoutDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        var response = await CallDdeiAsync(
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

    public async Task CancelCheckoutDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        await CallDdeiAsync(_mdsClientRequestFactory.CreateCancelCheckoutDocumentRequest(arg), arg.CmsAuthValues);
    }

    public async Task<HttpResponseMessage> UploadPdfAsync(DdeiDocumentIdAndVersionIdArgDto arg, Stream stream)
    {
        return await CallDdeiAsync(_mdsClientRequestFactory.CreateUploadPdfRequest(arg, stream), arg.CmsAuthValues,
        [
            HttpStatusCode.Gone,
            HttpStatusCode.RequestEntityTooLarge
        ]);
    }

    public async Task<IEnumerable<DocumentNoteDto>> GetDocumentNotesAsync(DdeiDocumentArgDto arg)
    {
        var ddeiResults = await CallDdeiAsync<List<DocumentNoteResponse>>(_mdsClientRequestFactory.CreateGetDocumentNotesRequest(arg), arg.CmsAuthValues);

        return ddeiResults.Select(ddeiResult => _caseDocumentNoteMapper.Map(ddeiResult)).ToArray();
    }

    public async Task<DocumentNoteResult> AddDocumentNoteAsync(DdeiAddDocumentNoteArgDto arg)
    {
        var response = await CallDdeiAsync<DdeiDocumentNoteAddedResponse>(_mdsClientRequestFactory.CreateAddDocumentNoteRequest(arg), arg.CmsAuthValues);

        return _caseDocumentNoteResultMapper.Map(response);
    }

    public async Task<DocumentRenamedResultDto> RenameDocumentAsync(DdeiRenameDocumentArgDto arg)
    {
        var response = await CallDdeiAsync<RenameMaterialResponse>(_mdsClientRequestFactory.CreateRenameDocumentRequest(arg), arg.CmsAuthValues);

        return new DocumentRenamedResultDto { Id = response.UpdateCommunication.Id };
    }

    public async Task<DocumentRenamedResultDto> RenameExhibitAsync(DdeiRenameDocumentArgDto arg)
    {
        var response = await CallDdeiAsync<RenameMaterialDescriptionResponse>(_mdsClientRequestFactory.CreateRenameExhibitRequest(arg), arg.CmsAuthValues);

        return new DocumentRenamedResultDto { Id = response.UpdateCommunicationDescription.Id };
    }

    public async Task<DocumentReclassifiedResultDto> ReclassifyDocumentAsync(DdeiReclassifyDocumentArgDto arg)
    {
        var response = await CallDdeiAsync<DdeiDocumentReclassifiedResponse>(_mdsClientRequestFactory.CreateReclassifyDocumentRequest(arg), arg.CmsAuthValues);

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

    public async Task<DdeiCommunicationReclassifiedResponse> ReclassifyCommunicationAsync(DdeiReclassifyCommunicationArgDto arg)
    {
        return await CallDdeiAsync<DdeiCommunicationReclassifiedResponse>(_mdsClientRequestFactory.CreateReclassifyCommunicationRequest(arg), arg.CmsAuthValues);
    }

    public async Task<IEnumerable<ExhibitProducerDto>> GetExhibitProducersAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var ddeiResults = await CallDdeiAsync<MdsDocumentExhibitProducerResponse>(_mdsClientRequestFactory.CreateGetExhibitProducersRequest(arg), arg.CmsAuthValues);

        return ddeiResults.ExhibitProducers.Select(_caseExhibitProducerMapper.Map);
    }

    public async Task<IEnumerable<BaseCaseWitnessResponse>> GetWitnessesAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var ddeiResults = await CallDdeiAsync<List<MdsCaseWitnessResponse>>(_mdsClientRequestFactory.CreateCaseWitnessesRequest(arg), arg.CmsAuthValues);

        return ddeiResults;
    }

    public async Task<IEnumerable<MaterialTypeDto>> GetMaterialTypeListAsync(DdeiBaseArgDto arg)
    {
        var ddeiResults = await CallDdeiAsync<List<MdsMaterialTypeListResponse>>(_mdsClientRequestFactory.CreateGetMaterialTypeListRequest(arg), arg.CmsAuthValues);

        return ddeiResults.Select(ddeiResult => _cmsMaterialTypeMapper.Map(ddeiResult)).ToArray();
    }

    public async Task<IEnumerable<WitnessStatementDto>> GetWitnessStatementsAsync(DdeiWitnessStatementsArgDto arg)
    {
        var ddeiResults = await CallDdeiAsync<StatementsForWitnessResponse>(_mdsClientRequestFactory.CreateGetWitnessStatementsRequest(arg), arg.CmsAuthValues);

        return ddeiResults.StatementsForWitness.Select(_caseWitnessStatementMapper.Map).ToArray();
    }

    public async Task<bool> ToggleIsUnusedDocumentAsync(DdeiToggleIsUnusedDocumentDto toggleIsUnusedDocumentDto) =>
        (await CallDdeiAsync(_mdsClientRequestFactory.CreateToggleIsUnusedDocumentRequest(toggleIsUnusedDocumentDto), toggleIsUnusedDocumentDto.CmsAuthValues))
        .IsSuccessStatusCode;

    public async Task<IEnumerable<DdeiCaseIdentifiersDto>> ListCaseIdsAsync(DdeiUrnArgDto arg) =>
        await CallDdeiAsync<IEnumerable<DdeiCaseIdentifiersDto>>(_mdsClientRequestFactory.CreateListCasesRequest(arg), arg.CmsAuthValues);

    private async Task<DdeiCaseDetailsDto> GetCaseInternalAsync(DdeiCaseIdentifiersArgDto arg) =>
        await CallDdeiAsync<DdeiCaseDetailsDto>(_mdsClientRequestFactory.CreateGetCaseRequest(arg), arg.CmsAuthValues);

    protected override HttpClient GetHttpClient(string cmsAuthValues)
    {
        var mdsClientName = _mdsClientFactory.Create(cmsAuthValues);
        return _httpClientFactory.CreateClient(mdsClientName);
    }
}