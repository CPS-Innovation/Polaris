namespace coordinator.Domain.Tracker
{
    public enum TrackerStatus
    {
        Initialised,
        NotStarted,
        Running,

        // Another Status here for document list ready
        NoDocumentsFoundInDDEI,
        Completed,
        Failed,
        UnableToEvaluateExistingDocuments
    }
}

