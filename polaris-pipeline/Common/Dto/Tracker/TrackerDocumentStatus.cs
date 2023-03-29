namespace Common.Dto.Tracker
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