using Common.Constants;
using System.IO;

namespace PolarisGateway.Services.Artefact.Domain
{
    public class DocumentRetrievalResult
    {
        public Stream PdfStream { get; set; }
        public PdfConversionStatus Status { get; set; }
    }
}