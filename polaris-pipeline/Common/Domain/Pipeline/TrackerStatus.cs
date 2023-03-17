namespace Common.Domain.Pipeline
{
    public enum TrackerStatus
    {
        Initialised,
        NotStarted,
        Running,
        NoDocumentsFoundInDDEI,
        DocumentsRetrieved,
        Completed,
        Failed,
        UnableToEvaluateExistingDocuments
    }
}

