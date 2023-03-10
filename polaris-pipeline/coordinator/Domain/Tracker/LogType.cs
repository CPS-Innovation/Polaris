namespace coordinator.Domain.Tracker
{
    public enum LogType
    {
        Initialised,
        RegisteredDocumentIds,
        RegisteredPdfBlobName,
        DocumentAlreadyProcessed,
        UnableToConvertDocumentToPdf,
        DocumentNotFoundInDDEI,
        DocumentRetrieved,
        UnexpectedDocumentFailure,
        NoDocumentsFoundInDDEI,
        Indexed,
        OcrAndIndexFailure,
        Completed,
        Failed, 
        ProcessedEvaluatedDocuments
    }
}
