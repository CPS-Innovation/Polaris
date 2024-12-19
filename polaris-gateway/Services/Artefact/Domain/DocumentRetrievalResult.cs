using Common.Constants;

namespace PolarisGateway.Services.Artefact.Domain
{
    public class DocumentRetrievalResult
    {
        public Stream PdfStream { get; set; }
        public PdfConversionStatus Status { get; set; }
    }
}