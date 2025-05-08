using Common.Dto.Response.Document;
using Common.Wrappers;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.Response.Document;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Domain.Response.Document;
using DdeiClient.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DdeiClient.Clients;

public class MdsClient : BaseDdeiClient
{
    public MdsClient(
        [FromKeyedServices(nameof(DdeiClients.Mds))] HttpClient httpClient,
        [FromKeyedServices(DdeiClients.Mds)] IDdeiClientRequestFactory ddeiClientRequestFactory,
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
        ILogger<MdsClient> logger) :
        base(httpClient,
            ddeiClientRequestFactory,
            caseDataServiceArgFactory,
            caseDetailsMapper,
            caseDocumentMapper,
            caseDocumentNoteMapper,
            caseDocumentNoteResultMapper,
            caseExhibitProducerMapper,
            caseWitnessMapper,
            caseIdentifiersMapper,
            cmsMaterialTypeMapper,
            caseWitnessStatementMapper,
            jsonConvertWrapper,
            logger)
    {
    }

    public override async Task<DocumentRenamedResultDto> RenameDocumentAsync(DdeiRenameDocumentArgDto arg)
    {
        var response = await CallDdei<RenameMaterialResponse>(DdeiClientRequestFactory.CreateRenameDocumentRequest(arg));

        return new DocumentRenamedResultDto { Id = response.UpdateCommunication.Id };
    }
}