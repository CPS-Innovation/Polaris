namespace Common.Dto.Tracker
{
    public enum TrackerLogType
    {
        Initialised,
        DocumentsSynchronised,
        RegisteredPdfBlobName,
        DocumentAlreadyProcessed,
        UnableToConvertDocumentToPdf,
        CmsDocumentCreated,
        CmsDocumentUpdated,
        CmsDocumentDeleted,
        PcdRequestCreated,
        PcdRequestUpdated,
        PcdRequestDeleted,
        DefendantAndChargesCreated,
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
