namespace coordinator.Domain.Tracker
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

