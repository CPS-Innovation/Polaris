using Common.Dto.Response.Documents;
using coordinator.Domain;
using coordinator.Services;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;

namespace coordinator.Durable.Activity;

public class SetDocumentRefreshStatuses(IStateStorageService stateStorageService) : BaseDocumentUpdateActivity
{
    [Function(nameof(SetDocumentRefreshStatuses))]
    public async Task<bool> Run([ActivityTrigger] SetDocumentRefreshStatusesPayload payload)
    {
        var documentsState = await stateStorageService.GetDurableEntityDocumentsStateAsync(payload.CaseId);

        foreach (var documentResult in payload.DocumentRefreshStatuses)
        {
            var document = GetDocument(documentResult.DocumentId, documentsState);

            document.ConversionStatus = documentResult.PdfConversionResponse.PdfConversionStatus;

            if (!documentResult.PdfConversionResponse.BlobAlreadyExists)
            {
                document.Status = DocumentStatus.UnableToConvertToPdf;
            }
            else
            {
                document.Status = DocumentStatus.PdfUploadedToBlob;

                if (documentResult.StoreCaseIndexesResponse is not null)
                {

                    if (!documentResult.StoreCaseIndexesResponse.IsSuccess)
                    {
                        document.Status = DocumentStatus.OcrAndIndexFailure;
                    }
                    else
                    {
                        document.Status = DocumentStatus.Indexed;
                    }
                }
            }
        }

        return await stateStorageService.UpdateDurableEntityDocumentsStateAsync(payload.CaseId, documentsState);
    }
}
