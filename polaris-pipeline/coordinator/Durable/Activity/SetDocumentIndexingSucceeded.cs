using Common.Dto.Response.Documents;
using coordinator.Domain;
using coordinator.Services;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;

namespace coordinator.Durable.Activity;

public class SetDocumentIndexingSucceeded(IStateStorageService stateStorageService) : BaseDocumentUpdateActivity
{
    [Function(nameof(SetDocumentIndexingSucceeded))]
    public async Task<bool> Run([ActivityTrigger] CaseIdAndDocumentIPayload payload)
    {
        var documentsState = await stateStorageService.GetDurableEntityDocumentsStateAsync(payload.CaseId);
        var document = GetDocument(payload.DocumentId, documentsState);
        document.Status = DocumentStatus.Indexed;

        return await stateStorageService.UpdateDurableEntityDocumentsStateAsync(payload.CaseId, documentsState);
    }
}
