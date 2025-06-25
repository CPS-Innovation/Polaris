using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Case;
using Common.Dto.Response.Document;
using Common.Dto.Response;
using Common.Wrappers;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.Response.Defendant;
using Ddei.Domain.Response;
using Ddei.Domain.Response.Document;
using Ddei.Domain.Response.PreCharge;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Domain.Args;
using DdeiClient.Domain.Response.Document;
using DdeiClient.Domain.Response;
using DdeiClient.Enums;
using DdeiClient.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using DdeiClient.Clients.Interfaces;

namespace DdeiClient.Clients;

public class MdsClient : BaseMdsClient, IMdsClient
{
    private readonly IHttpClientFactory HttpClientFactory;
    private readonly IMdsClientRequestFactory MdsClientRequestFactory;
    private readonly IDdeiArgFactory CaseDataServiceArgFactory;
    private readonly ICaseDetailsMapper CaseDetailsMapper;
    private readonly ICaseDocumentMapper<DdeiDocumentResponse> CaseDocumentMapper;
    private readonly ICaseDocumentNoteMapper CaseDocumentNoteMapper;
    private readonly ICaseDocumentNoteResultMapper CaseDocumentNoteResultMapper;
    private readonly ICaseExhibitProducerMapper caseExhibitProducerMapper;
    private readonly ICaseIdentifiersMapper caseIdentifiersMapper;
    private readonly ICmsMaterialTypeMapper cmsMaterialTypeMapper;
    private readonly ICaseWitnessStatementMapper caseWitnessStatementMapper;
    private readonly IJsonConvertWrapper jsonConvertWrapper;
    private readonly ILogger<MdsClient> logger;
    private IMdsClientFactory mdsClientFactory;
    public MdsClient(
        ) :
        base(null)
    {
    }
    public virtual async Task<CaseIdentifiersDto> GetUrnFromCaseIdAsync(DdeiCaseIdOnlyArgDto arg)
    {
        var response = await CallDdeiAsync<DdeiCaseIdentifiersDto>(MdsClientRequestFactory.CreateUrnLookupRequest(arg), arg.CmsAuthValues);

        return CaseIdentifiersMapper.MapCaseIdentifiers(response);
    }

    public virtual async Task<IEnumerable<CaseDto>> ListCasesAsync(DdeiUrnArgDto arg)
    {
        var caseIdentifiers = await ListCaseIdsAsync(arg);

        var calls = caseIdentifiers.Select(async caseIdentifier =>
            await GetCaseInternalAsync(CaseDataServiceArgFactory.CreateCaseArgFromUrnArg(arg, caseIdentifier.Id)));

        var cases = await Task.WhenAll(calls);
        return cases.Select(@case => CaseDetailsMapper.MapCaseDetails(@case));
    }

    public virtual async Task<CaseDto> GetCaseAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var @case = await GetCaseInternalAsync(arg);
        return CaseDetailsMapper.MapCaseDetails(@case);
    }

    public virtual async Task<CaseSummaryDto> GetCaseSummaryAsync(DdeiCaseIdOnlyArgDto arg)
    {
        var ddeiResult = await CallDdeiAsync<MdsCaseSummaryDto>(MdsClientRequestFactory.CreateGetCaseSummary(arg), arg.CmsAuthValues);
        return CaseDetailsMapper.Map(ddeiResult);
    }

    public virtual async Task<IEnumerable<PcdRequestCoreDto>> GetPcdRequestsCoreAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var pcdRequests = await CallDdeiAsync<IEnumerable<DdeiPcdRequestCoreDto>>(MdsClientRequestFactory.CreateGetPcdRequestsRequest(arg), arg.CmsAuthValues);
        return CaseDetailsMapper.MapCorePreChargeDecisionRequests(pcdRequests);
    }

    public virtual async Task<IEnumerable<PcdRequestDto>> GetPcdRequestsAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var pcdRequests = await CallDdeiAsync<IEnumerable<DdeiPcdRequestDto>>(MdsClientRequestFactory.CreateGetPcdRequestsRequest(arg), arg.CmsAuthValues);
        return CaseDetailsMapper.MapPreChargeDecisionRequests(pcdRequests);
    }

    public virtual async Task<PcdRequestDto> GetPcdRequestAsync(DdeiPcdArgDto arg)
    {
        var pcdRequest = await CallDdeiAsync<DdeiPcdRequestDto>(MdsClientRequestFactory.CreateGetPcdRequest(arg), arg.CmsAuthValues);
        return CaseDetailsMapper.MapPreChargeDecisionRequest(pcdRequest);
    }

    public virtual async Task<DefendantsAndChargesListDto> GetDefendantAndChargesAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var response = await CallDdeiAsync(MdsClientRequestFactory.CreateGetDefendantAndChargesRequest(arg), arg.CmsAuthValues);
        var content = await response.Content.ReadAsStringAsync();
        var defendantAndCharges = JsonConvertWrapper.DeserializeObject<IEnumerable<DdeiCaseDefendantDto>>(content);
        var etag = response.Headers.ETag?.Tag;

        return CaseDetailsMapper.MapDefendantsAndCharges(defendantAndCharges, arg.CaseId, etag);
    }

    public virtual async Task<IEnumerable<CmsDocumentDto>> ListDocumentsAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var ddeiResults = await CallDdeiAsync<List<DdeiDocumentResponse>>(MdsClientRequestFactory.CreateListCaseDocumentsRequest(arg), arg.CmsAuthValues);

        return ddeiResults.Select(ddeiResult => CaseDocumentMapper.Map(ddeiResult));
    }

    public virtual async Task<FileResult> GetDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        var response = await CallDdeiAsync(MdsClientRequestFactory.CreateGetDocumentRequest(arg), arg.CmsAuthValues);
        var fileName = response.Content.Headers.GetValues("Content-Disposition").ToList()[0];

        return new FileResult
        {
            Stream = await response.Content.ReadAsStreamAsync(),
            FileName = fileName
        };
    }

    public virtual async Task<Stream> GetDocumentFromFileStoreAsync(string path, string cmsAuthValues, Guid correlationId)
    {
        var response = await CallDdeiAsync(
            MdsClientRequestFactory.CreateDocumentFromFileStoreRequest(new DdeiFileStoreArgDto
            {
                Path = path,
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId
            }),
            cmsAuthValues
        );

        return await response.Content.ReadAsStreamAsync();
    }

    public virtual async Task<CheckoutDocumentDto> CheckoutDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        var response = await CallDdeiAsync(
            MdsClientRequestFactory.CreateCheckoutDocumentRequest(arg), arg.CmsAuthValues, HttpStatusCode.Conflict);

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

    public virtual async Task CancelCheckoutDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg)
    {
        await CallDdeiAsync(MdsClientRequestFactory.CreateCancelCheckoutDocumentRequest(arg), arg.CmsAuthValues);
    }

    public virtual async Task<HttpResponseMessage> UploadPdfAsync(DdeiDocumentIdAndVersionIdArgDto arg, Stream stream)
    {
        return await CallDdeiAsync(MdsClientRequestFactory.CreateUploadPdfRequest(arg, stream), arg.CmsAuthValues,
        [
            HttpStatusCode.Gone,
            HttpStatusCode.RequestEntityTooLarge
        ]);
    }

    public virtual async Task<IEnumerable<DocumentNoteDto>> GetDocumentNotesAsync(DdeiDocumentArgDto arg)
    {
        var ddeiResults = await CallDdeiAsync<List<DocumentNoteResponse>>(MdsClientRequestFactory.CreateGetDocumentNotesRequest(arg), arg.CmsAuthValues);

        return ddeiResults.Select(ddeiResult => CaseDocumentNoteMapper.Map(ddeiResult)).ToArray();
    }

    public virtual async Task<DocumentNoteResult> AddDocumentNoteAsync(DdeiAddDocumentNoteArgDto arg)
    {
        var response = await CallDdeiAsync<DdeiDocumentNoteAddedResponse>(MdsClientRequestFactory.CreateAddDocumentNoteRequest(arg), arg.CmsAuthValues);

        return CaseDocumentNoteResultMapper.Map(response);
    }

    public virtual async Task<DocumentRenamedResultDto> RenameDocumentAsync(DdeiRenameDocumentArgDto arg)
    {
        var response = await CallDdeiAsync<RenameMaterialResponse>(MdsClientRequestFactory.CreateRenameDocumentRequest(arg), arg.CmsAuthValues);

        return new DocumentRenamedResultDto { Id = response.UpdateCommunication.Id };
    }

    public virtual async Task<DocumentRenamedResultDto> RenameExhibitAsync(DdeiRenameDocumentArgDto arg)
    {
        var response = await CallDdeiAsync<RenameMaterialDescriptionResponse>(MdsClientRequestFactory.CreateRenameExhibitRequest(arg), arg.CmsAuthValues);

        return new DocumentRenamedResultDto { Id = response.UpdateCommunicationDescription.Id };
    }

    public virtual async Task<DocumentReclassifiedResultDto> ReclassifyDocumentAsync(DdeiReclassifyDocumentArgDto arg)
    {
        var response = await CallDdeiAsync<DdeiDocumentReclassifiedResponse>(MdsClientRequestFactory.CreateReclassifyDocumentRequest(arg), arg.CmsAuthValues);

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

    public virtual async Task<DdeiCommunicationReclassifiedResponse> ReclassifyCommunicationAsync(DdeiReclassifyCommunicationArgDto arg)
    {
        return await CallDdeiAsync<DdeiCommunicationReclassifiedResponse>(MdsClientRequestFactory.CreateReclassifyCommunicationRequest(arg), arg.CmsAuthValues);
    }

    public virtual async Task<IEnumerable<ExhibitProducerDto>> GetExhibitProducersAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var ddeiResults = await CallDdeiAsync<MdsDocumentExhibitProducerResponse>(MdsClientRequestFactory.CreateGetExhibitProducersRequest(arg), arg.CmsAuthValues);

        return ddeiResults.ExhibitProducers.Select(CaseExhibitProducerMapper.Map);
    }

    public virtual async Task<IEnumerable<BaseCaseWitnessResponse>> GetWitnessesAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var ddeiResults = await CallDdeiAsync<List<MdsCaseWitnessResponse>>(MdsClientRequestFactory.CreateCaseWitnessesRequest(arg), arg.CmsAuthValues);

        return ddeiResults;
    }

    public virtual async Task<IEnumerable<MaterialTypeDto>> GetMaterialTypeListAsync(DdeiBaseArgDto arg)
    {
        var ddeiResults = await CallDdeiAsync<List<MdsMaterialTypeListResponse>>(MdsClientRequestFactory.CreateGetMaterialTypeListRequest(arg), arg.CmsAuthValues);

        return ddeiResults.Select(ddeiResult => CmsMaterialTypeMapper.Map(ddeiResult)).ToArray();
    }

    public virtual async Task<IEnumerable<WitnessStatementDto>> GetWitnessStatementsAsync(DdeiWitnessStatementsArgDto arg)
    {
        var ddeiResults = await CallDdeiAsync<StatementsForWitnessResponse>(MdsClientRequestFactory.CreateGetWitnessStatementsRequest(arg), arg.CmsAuthValues);

        return ddeiResults.StatementsForWitness.Select(CaseWitnessStatementMapper.Map).ToArray();
    }

    public virtual async Task<bool> ToggleIsUnusedDocumentAsync(DdeiToggleIsUnusedDocumentDto toggleIsUnusedDocumentDto) =>
        (await CallDdeiAsync(MdsClientRequestFactory.CreateToggleIsUnusedDocumentRequest(toggleIsUnusedDocumentDto), toggleIsUnusedDocumentDto.CmsAuthValues))
        .IsSuccessStatusCode;

    public virtual async Task<IEnumerable<DdeiCaseIdentifiersDto>> ListCaseIdsAsync(DdeiUrnArgDto arg) =>
        await CallDdeiAsync<IEnumerable<DdeiCaseIdentifiersDto>>(MdsClientRequestFactory.CreateListCasesRequest(arg), arg.CmsAuthValues);

    protected virtual async Task<DdeiCaseDetailsDto> GetCaseInternalAsync(DdeiCaseIdentifiersArgDto arg) =>
        await CallDdeiAsync<DdeiCaseDetailsDto>(MdsClientRequestFactory.CreateGetCaseRequest(arg), arg.CmsAuthValues);
}