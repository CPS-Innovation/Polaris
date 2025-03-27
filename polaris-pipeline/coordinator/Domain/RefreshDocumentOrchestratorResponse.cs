using Common.Dto.Response;

namespace coordinator.Domain;

public class RefreshDocumentOrchestratorResponse
{
    public RefreshDocumentOrchestratorResponse()
    {
    }

    public string DocumentId { get; set; }

    public PdfConversionResponse PdfConversionResponse { get; set; }

    public StoreCaseIndexesResult StoreCaseIndexesResponse { get; set; }
}
