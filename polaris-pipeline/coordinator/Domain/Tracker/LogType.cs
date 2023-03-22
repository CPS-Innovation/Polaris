namespace coordinator.Domain.Tracker
{
    public enum LogType
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
