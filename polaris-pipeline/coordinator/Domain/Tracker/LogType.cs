namespace coordinator.Domain.Tracker
{
    public enum LogType
    {
        Initialised,
        RegisteredDocumentIds,
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
