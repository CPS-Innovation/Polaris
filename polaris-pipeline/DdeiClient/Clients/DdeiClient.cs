using Common.Dto.Response;
using Common.Dto.Response.Case;
using Common.Dto.Response.Document;
using Common.Wrappers;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Domain.Response;
using Ddei.Domain.Response.Document;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Domain.Response;
using DdeiClient.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DdeiClient.Clients;

public class DdeiClient : BaseDdeiClient
{
    public DdeiClient(
        [FromKeyedServices(nameof(DdeiClients.Ddei))] HttpClient httpClient,
        [FromKeyedServices(DdeiClients.Ddei)] IDdeiClientRequestFactory ddeiClientRequestFactory,
        IDdeiArgFactory caseDataServiceArgFactory,
        ICaseDetailsMapper caseDetailsMapper,
        ICaseDocumentMapper<DdeiDocumentResponse> caseDocumentMapper,
        ICaseDocumentNoteMapper caseDocumentNoteMapper,
        ICaseDocumentNoteResultMapper caseDocumentNoteResultMapper,
        ICaseExhibitProducerMapper caseExhibitProducerMapper,
        ICaseIdentifiersMapper caseIdentifiersMapper,
        ICmsMaterialTypeMapper cmsMaterialTypeMapper,
        ICaseWitnessStatementMapper caseWitnessStatementMapper,
        IJsonConvertWrapper jsonConvertWrapper,
        ILogger<DdeiClient> logger) :
        base(httpClient,
            ddeiClientRequestFactory,
            caseDataServiceArgFactory,
            caseDetailsMapper,
            caseDocumentMapper,
            caseDocumentNoteMapper,
            caseDocumentNoteResultMapper,
            caseExhibitProducerMapper,
            caseIdentifiersMapper,
            cmsMaterialTypeMapper,
            caseWitnessStatementMapper,
            jsonConvertWrapper,
            logger)
    {
    }

    public override async Task<DocumentRenamedResultDto> RenameDocumentAsync(DdeiRenameDocumentArgDto arg)
    {
        var response = await CallDdei<DdeiDocumentRenamedResponse>(DdeiClientRequestFactory.CreateRenameDocumentRequest(arg));

        return new DocumentRenamedResultDto { Id = response.Id, OperationName = response.OperationName };
    }

    public override async Task<IEnumerable<WitnessStatementDto>> GetWitnessStatementsAsync(DdeiWitnessStatementsArgDto arg)
    {
        var ddeiResults = await CallDdei<List<DdeiCaseWitnessStatementsResponse>>(DdeiClientRequestFactory.CreateGetWitnessStatementsRequest(arg));

        return ddeiResults.Select(ddeiResult => CaseWitnessStatementMapper.Map(ddeiResult)).ToArray();
    }

    public override async Task<IEnumerable<DocumentNoteDto>> GetDocumentNotesAsync(DdeiDocumentArgDto arg)
    {
        var ddeiResults = await CallDdei<List<DdeiDocumentNoteResponse>>(DdeiClientRequestFactory.CreateGetDocumentNotesRequest(arg));

        return ddeiResults.Select(ddeiResult => CaseDocumentNoteMapper.Map(ddeiResult)).ToArray();
    }

    public override async Task<IEnumerable<BaseCaseWitnessResponse>> GetWitnessesAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var ddeiResults = await CallDdei<List<DdeiCaseWitnessResponse>>(DdeiClientRequestFactory.CreateCaseWitnessesRequest(arg));

        return ddeiResults;
    }

    public override async Task<IEnumerable<CaseDto>> ListCasesAsync(DdeiUrnArgDto arg)
    {
        var caseIdentifiers = await ListCaseIdsAsync(arg);

        var calls = caseIdentifiers.Select(async caseIdentifier =>
            await GetCaseInternalAsync(CaseDataServiceArgFactory.CreateCaseArgFromUrnArg(arg, caseIdentifier.Id)));

        var cases = await Task.WhenAll(calls);
        return cases.Select(@case => CaseDetailsMapper.MapCaseDetails(@case));
    }

    public override async Task<CaseDto> GetCaseAsync(DdeiCaseIdentifiersArgDto arg)
    {
        var @case = await GetCaseInternalAsync(arg);
        return CaseDetailsMapper.MapCaseDetails(@case);
    }

    public override async Task<IEnumerable<MaterialTypeDto>> GetMaterialTypeListAsync(DdeiBaseArgDto arg)
    {
        var ddeiResults = await CallDdei<List<DdeiMaterialTypeListResponse>>(DdeiClientRequestFactory.CreateGetMaterialTypeListRequest(arg));

        return ddeiResults.Select(ddeiResult => CmsMaterialTypeMapper.Map(ddeiResult)).ToArray();
    }

    private new async Task<DdeiCaseDetailsDto> GetCaseInternalAsync(DdeiCaseIdentifiersArgDto arg) =>
        await CallDdei<DdeiCaseDetailsDto>(DdeiClientRequestFactory.CreateGetCaseRequest(arg));
}