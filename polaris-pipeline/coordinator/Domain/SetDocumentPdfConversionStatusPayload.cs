using Common.Constants;

namespace coordinator.Domain;

public class SetDocumentPdfConversionStatusPayload
{
    required public int CaseId { get; set; }

    public string DocumentId { get; set; }

    public PdfConversionStatus PdfConversionStatus { get; set; }
}