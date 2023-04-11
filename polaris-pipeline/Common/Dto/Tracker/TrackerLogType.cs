namespace Common.Dto.Tracker
{
    public enum TrackerLogType
    {
        Initialised,
        DocumentsSynchronised,
        RegisteredPdfBlobName,
        DocumentAlreadyProcessed,
        UnableToConvertDocumentToPdf,
        UnableToConvertPcdRequestToPdf,
        DocumentRetrieved,
        PcdRequestRetrieved,
        UnexpectedDocumentFailure,
        Indexed,
        OcrAndIndexFailure,
        Completed,
        Failed,
        ProcessedEvaluatedDocuments,
        DeletedDocument,
        DeletedPcdRequest
    }
}
