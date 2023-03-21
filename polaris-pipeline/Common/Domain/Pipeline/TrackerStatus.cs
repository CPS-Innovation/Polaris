namespace Common.Domain.Pipeline
{
    public enum TrackerStatus
    {
        Running,
        NoDocumentsFoundInDDEI,
        DocumentsRetrieved,
        Completed,
        Failed,
        Deleted
    }
}

