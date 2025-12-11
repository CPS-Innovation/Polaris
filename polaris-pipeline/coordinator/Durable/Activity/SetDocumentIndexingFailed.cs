using Common.Dto.Response.Documents;
using coordinator.Domain;
using coordinator.Services;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;

namespace coordinator.Durable.Activity;

public class SetDocumentIndexingFailed(IStateStorageService stateStorageService) : BaseDocumentUpdateActivity
{
    [Function(nameof(SetDocumentIndexingFailed))]
    public async Task<bool> Run([ActivityTrigger] CaseIdAndDocumentIdPayload payload)
    {
        var documentsState = await stateStorageService.GetDurableEntityDocumentsStateAsync(payload.CaseId);
        var document = GetDocument(payload.DocumentId, documentsState);
        document.Status = DocumentStatus.OcrAndIndexFailure;

        return await stateStorageService.UpdateDurableEntityDocumentsStateAsync(payload.CaseId, documentsState);
    }
}
