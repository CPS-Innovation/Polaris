using Common.Constants;

namespace coordinator.Domain;

public class PdfConversionResponse
{
    public bool BlobAlreadyExists { get; set; }

    public PdfConversionStatus PdfConversionStatus { get; set; }
}