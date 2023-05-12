namespace Common.Dto.Tracker
{
    public enum TrackerLogType
    {
        Initialised,
        DocumentsSynchronised,
        RegisteredPdfBlobName,
        DocumentAlreadyProcessed,
        UnableToConvertDocumentToPdf,
        CmsDocumentRetrieved,
        CmsDocumentUpdated,
        CmsDocumentDeleted,
        PcdRequestRetrieved,
        PcdRequestUpdated,
        PcdRequestDeleted,
        DefendantAndChargesRetrieved,
        DefendantAndChargesUpdated,
        DefendantAndChargesDeleted,
        UnexpectedDocumentFailure,
        Indexed,
        OcrAndIndexFailure,
        Completed,
        Failed,
        ProcessedEvaluatedDocuments
    }
}
