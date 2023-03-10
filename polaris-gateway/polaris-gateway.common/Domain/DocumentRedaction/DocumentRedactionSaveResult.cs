namespace PolarisGateway.Domain.DocumentRedaction
{
    public class DocumentRedactionSaveResult
    {
        public bool Succeeded { get; set; }

        public string RedactedDocumentUrl { get; set; }

        public string Message { get; set; }
    }
}
