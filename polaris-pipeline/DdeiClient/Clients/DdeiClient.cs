using Common.Wrappers;
using Ddei.Domain.Response.Document;
using Ddei.Factories;
using Ddei.Mappers;
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
        ICaseWitnessMapper caseWitnessMapper,
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
            caseWitnessMapper,
            caseIdentifiersMapper,
            cmsMaterialTypeMapper,
            caseWitnessStatementMapper,
            jsonConvertWrapper,
            logger)
    {
    }
}