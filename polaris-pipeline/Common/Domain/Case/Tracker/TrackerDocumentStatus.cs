namespace Common.Domain.Case.Tracker
{
    public enum TrackerDocumentStatus
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