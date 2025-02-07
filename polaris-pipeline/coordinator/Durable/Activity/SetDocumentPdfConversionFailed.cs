using Common.Dto.Response.Documents;
using coordinator.Domain;
using coordinator.Services;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;

namespace coordinator.Durable.Activity;

public class SetDocumentPdfConversionFailed(IStateStorageService stateStorageService) : BaseDocumentUpdateActivity
{
    [Function(nameof(SetDocumentPdfConversionFailed))]
    public async Task<bool> Run([ActivityTrigger] SetDocumentPdfConversionStatusPayload payload)
    {
        var documentsState = await stateStorageService.GetDurableEntityDocumentsStateAsync(payload.CaseId);
        var document = GetDocument(payload.DocumentId, documentsState);
        document.Status = DocumentStatus.UnableToConvertToPdf;
        document.ConversionStatus = payload.PdfConversionStatus;

        return await stateStorageService.UpdateDurableEntityDocumentsStateAsync(payload.CaseId, documentsState);
    }
}
