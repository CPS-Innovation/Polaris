namespace polaris_common.Dto.Response
{
    public class RedactPdfResponse
    {
        public bool Succeeded { get; set; }

        public string RedactedDocumentName { get; set; }

        public string Message { get; set; }
    }
}
