namespace Common.Dto.Tracker
{
    public enum TrackerLogType
    {
        Initialised,
        DocumentsSynchronised,
        RegisteredPdfBlobName,
        DocumentAlreadyProcessed,
        UnableToConvertDocumentToPdf,
        DocumentRetrieved,
        UnexpectedDocumentFailure,
        Indexed,
        OcrAndIndexFailure,
        Completed,
        Failed,
        ProcessedEvaluatedDocuments,
        Deleted
    }
}
