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
using System.Threading;

namespace DdeiClient.Clients;

public class MdsClient : BaseCmsClient, IMdsClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMdsClientRequestFactory _mdsClientRequestFactory;
    private readonly ICaseDetailsMapper _caseDetailsMapper;
    private readonly ICaseDocumentMapper<MdsDocumentResponse> _caseDocumentMapper;
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
        ICaseDetailsMapper caseDetailsMapper,
        ICaseDocumentMapper<MdsDocumentResponse> caseDocumentMapper,
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

    public async Task<CaseIdentifiersDto> GetUrnFromCaseIdAsync(MdsCaseIdOnlyArgDto arg, CancellationToken cancellationToken = default)
    {
        var mdsCaseIdentifiersDto = await CallHttpClientAsync<MdsCaseIdentifiersDto>(_mdsClientRequestFactory.CreateUrnLookupRequest(arg), arg.CmsAuthValues, cancellationToken);

        return _caseIdentifiersMapper.MapCaseIdentifiers(mdsCaseIdentifiersDto);
    }

    public async Task<CaseSummaryDto> GetCaseSummaryAsync(MdsCaseIdOnlyArgDto arg, CancellationToken cancellationToken = default)
    {
        var mdsResult = await CallHttpClientAsync<MdsCaseSummaryDto>(_mdsClientRequestFactory.CreateGetCaseSummary(arg), arg.CmsAuthValues, cancellationToken);
        return _caseDetailsMapper.Map(mdsResult);
    }

    public async Task<IEnumerable<PcdRequestCoreDto>> GetPcdRequestsCoreAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default)
    {
        var pcdRequests = await CallHttpClientAsync<IEnumerable<MdsPcdRequestCoreDto>>(_mdsClientRequestFactory.CreateGetPcdRequestsRequest(arg), arg.CmsAuthValues, cancellationToken);
        return _caseDetailsMapper.MapCorePreChargeDecisionRequests(pcdRequests);
    }

    public async Task<IEnumerable<PcdRequestDto>> GetPcdRequestsAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default)
    {
        var pcdRequests = await CallHttpClientAsync<IEnumerable<MdsPcdRequestDto>>(_mdsClientRequestFactory.CreateGetPcdRequestsRequest(arg), arg.CmsAuthValues, cancellationToken);
        return _caseDetailsMapper.MapPreChargeDecisionRequests(pcdRequests);
    }

    public async Task<PcdRequestDto> GetPcdRequestAsync(MdsPcdArgDto arg, CancellationToken cancellationToken = default)
    {
        var pcdRequest = await CallHttpClientAsync<MdsPcdRequestDto>(_mdsClientRequestFactory.CreateGetPcdRequest(arg), arg.CmsAuthValues, cancellationToken);
        return _caseDetailsMapper.MapPreChargeDecisionRequest(pcdRequest);
    }

    public async Task<DefendantsAndChargesListDto> GetDefendantAndChargesAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default)
    {
        var response = await CallHttpClientAsync(_mdsClientRequestFactory.CreateGetDefendantAndChargesRequest(arg), arg.CmsAuthValues, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var defendantAndCharges = _jsonConvertWrapper.DeserializeObject<IEnumerable<MdsCaseDefendantDto>>(content);
        var etag = response.Headers.ETag?.Tag;

        return _caseDetailsMapper.MapDefendantsAndCharges(defendantAndCharges, arg.CaseId, etag);
    }

    public async Task<IEnumerable<CmsDocumentDto>> ListDocumentsAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default)
    {
        var mdsResults = await CallHttpClientAsync<List<MdsDocumentResponse>>(_mdsClientRequestFactory.CreateListCaseDocumentsRequest(arg), arg.CmsAuthValues, cancellationToken);

        return mdsResults.Select(mdsResult => _caseDocumentMapper.Map(mdsResult));
    }

    public async Task<FileResult> GetDocumentAsync(MdsMaterialIdAndDocumentIdArgDto arg, CancellationToken cancellationToken = default)
    {
        var response = await CallHttpClientAsync(_mdsClientRequestFactory.CreateGetDocumentRequest(arg), arg.CmsAuthValues, cancellationToken);
        var fileName = response.Content.Headers.GetValues("Content-Disposition").ToList()[0];

        return new FileResult
        {
            Stream = await response.Content.ReadAsStreamAsync(cancellationToken),
            FileName = fileName,
        };
    }

    public async Task<CheckoutDocumentDto> CheckoutDocumentAsync(MdsMaterialIdAndDocumentIdArgDto arg, CancellationToken cancellationToken = default)
    {
        await CallHttpClientAsync(
            _mdsClientRequestFactory.CreateCheckoutDocumentRequest(arg), arg.CmsAuthValues, cancellationToken);

        return new CheckoutDocumentDto { IsSuccess = true };
    }

    public async Task CancelCheckoutDocumentAsync(MdsMaterialIdAndDocumentIdArgDto arg, CancellationToken cancellationToken = default)
    {
        await CallHttpClientAsync(_mdsClientRequestFactory.CreateCancelCheckoutDocumentRequest(arg), arg.CmsAuthValues, cancellationToken);
    }

    public async Task<HttpResponseMessage> UploadPdfAsync(MdsMaterialIdAndDocumentIdArgDto arg, Stream stream, CancellationToken cancellationToken = default)
    {
        return await CallHttpClientAsync(_mdsClientRequestFactory.CreateUploadPdfRequest(arg, stream), arg.CmsAuthValues, cancellationToken,
        [
            HttpStatusCode.Gone,
            HttpStatusCode.RequestEntityTooLarge
        ]);
    }

    public async Task<IEnumerable<DocumentNoteDto>> GetDocumentNotesAsync(MdsDocumentArgDto arg, CancellationToken cancellationToken = default)
    {
        var mdsResults = await CallHttpClientAsync<List<DocumentNoteResponse>>(_mdsClientRequestFactory.CreateGetDocumentNotesRequest(arg), arg.CmsAuthValues, cancellationToken);

        return mdsResults.Select(mdsResult => _caseDocumentNoteMapper.Map(mdsResult)).ToArray();
    }

    public async Task<DocumentNoteResult> AddDocumentNoteAsync(MdsAddDocumentNoteArgDto arg, CancellationToken cancellationToken = default)
    {
        var response = await CallHttpClientAsync<MdsDocumentNoteAddedResponse>(_mdsClientRequestFactory.CreateAddDocumentNoteRequest(arg), arg.CmsAuthValues, cancellationToken);

        return _caseDocumentNoteResultMapper.Map(response);
    }

    public async Task<DocumentRenamedResultDto> RenameDocumentAsync(MdsRenameDocumentArgDto arg, CancellationToken cancellationToken = default)
    {
        var response = await CallHttpClientAsync<RenameMaterialResponse>(_mdsClientRequestFactory.CreateRenameDocumentRequest(arg), arg.CmsAuthValues, cancellationToken);

        return new DocumentRenamedResultDto { Id = response.UpdateCommunication.Id };
    }

    public async Task<DocumentRenamedResultDto> RenameExhibitAsync(MdsRenameDocumentArgDto arg, CancellationToken cancellationToken = default)
    {
        var response = await CallHttpClientAsync<RenameMaterialDescriptionResponse>(_mdsClientRequestFactory.CreateRenameExhibitRequest(arg), arg.CmsAuthValues, cancellationToken);

        return new DocumentRenamedResultDto { Id = response.UpdateCommunicationDescription.Id };
    }

    public async Task<MdsCommunicationReclassifiedResponse> ReclassifyCommunicationAsync(MdsReclassifyCommunicationArgDto arg, CancellationToken cancellationToken = default)
    {
        return await CallHttpClientAsync<MdsCommunicationReclassifiedResponse>(_mdsClientRequestFactory.CreateReclassifyCommunicationRequest(arg), arg.CmsAuthValues, cancellationToken);
    }

    public async Task<IEnumerable<ExhibitProducerDto>> GetExhibitProducersAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default)
    {
        var mdsResults = await CallHttpClientAsync<MdsDocumentExhibitProducerResponse>(_mdsClientRequestFactory.CreateGetExhibitProducersRequest(arg), arg.CmsAuthValues, cancellationToken);

        return mdsResults.ExhibitProducers.Select(_caseExhibitProducerMapper.Map);
    }

    public async Task<IEnumerable<BaseCaseWitnessResponse>> GetWitnessesAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default)
    {
        var mdsResults = await CallHttpClientAsync<List<MdsCaseWitnessResponse>>(_mdsClientRequestFactory.CreateCaseWitnessesRequest(arg), arg.CmsAuthValues, cancellationToken);

        return mdsResults;
    }

    public async Task<IEnumerable<MaterialTypeDto>> GetMaterialTypeListAsync(CmsBaseArgDto arg, CancellationToken cancellationToken = default)
    {
        var mdsResults = await CallHttpClientAsync<List<MdsMaterialTypeListResponse>>(_mdsClientRequestFactory.CreateGetMaterialTypeListRequest(arg), arg.CmsAuthValues, cancellationToken);

        return mdsResults.Select(mdsResult => _cmsMaterialTypeMapper.Map(mdsResult)).ToArray();
    }

    public async Task<IEnumerable<WitnessStatementDto>> GetWitnessStatementsAsync(MdsWitnessStatementsArgDto arg, CancellationToken cancellationToken = default)
    {
        var mdsResults = await CallHttpClientAsync<StatementsForWitnessResponse>(_mdsClientRequestFactory.CreateGetWitnessStatementsRequest(arg), arg.CmsAuthValues, cancellationToken);

        return mdsResults.StatementsForWitness.Select(_caseWitnessStatementMapper.Map).ToArray();
    }

    public async Task<bool> ToggleIsUnusedDocumentAsync(MdsToggleIsUnusedDocumentDto toggleIsUnusedDocumentDto, CancellationToken cancellationToken = default) =>
        (await CallHttpClientAsync(_mdsClientRequestFactory.CreateToggleIsUnusedDocumentRequest(toggleIsUnusedDocumentDto), toggleIsUnusedDocumentDto.CmsAuthValues, cancellationToken))
        .IsSuccessStatusCode;

    public async Task<IEnumerable<MdsCaseIdentifiersDto>> ListCaseIdsAsync(MdsUrnArgDto arg, CancellationToken cancellationToken = default) =>
        await CallHttpClientAsync<IEnumerable<MdsCaseIdentifiersDto>>(_mdsClientRequestFactory.CreateListCasesRequest(arg), arg.CmsAuthValues, cancellationToken);

    private async Task<MdsCaseDetailsDto> GetCaseInternalAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default) =>
        await CallHttpClientAsync<MdsCaseDetailsDto>(_mdsClientRequestFactory.CreateGetCaseRequest(arg), arg.CmsAuthValues, cancellationToken);

    protected override HttpClient GetHttpClient(string cmsAuthValues)
    {
        var mdsClientName = _mdsClientFactory.Create(cmsAuthValues);
        return _httpClientFactory.CreateClient(mdsClientName);
    }
}