namespace Common.Dto.Response.Documents
{
    public enum DocumentStatus
    {
        New,
        Indexed,
        PdfUploadedToBlob,
        UnableToConvertToPdf,
        OcrAndIndexFailure,
    }
}