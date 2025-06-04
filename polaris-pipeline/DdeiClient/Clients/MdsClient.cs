using Common.Wrappers;
using Ddei.Domain.Response.Document;
using Ddei.Factories;
using Ddei.Mappers;
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
            caseIdentifiersMapper,
            cmsMaterialTypeMapper,
            caseWitnessStatementMapper,
            jsonConvertWrapper,
            logger)
    {
    }
}