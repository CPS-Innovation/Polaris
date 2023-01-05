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
        UnexpectedDocumentFailure,
        NoDocumentsFoundInDDEI,
        Indexed,
        OcrAndIndexFailure,
        Completed,
        Failed, 
        ProcessedEvaluatedDocuments
    }
}
