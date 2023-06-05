namespace Common.Dto.Tracker
{
    public enum DocumentStatus
    {
        New,
        Indexed,
        PdfUploadedToBlob,
        UnableToConvertToPdf,
        UnexpectedFailure,
        OcrAndIndexFailure,
        DocumentAlreadyProcessed,
    }
}