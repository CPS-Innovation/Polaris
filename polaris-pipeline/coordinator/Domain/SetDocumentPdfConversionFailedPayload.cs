using Common.Constants;

namespace coordinator.Domain;

public class SetDocumentPdfConversionFailedPayload
{
    public string DocumentId { get; set; }

    public PdfConversionStatus PdfConversionStatus { get; set; }
}
