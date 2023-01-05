namespace coordinator.Domain.Tracker
{
	public enum TrackerStatus
	{
		Initialised,
		NotStarted,
		Running,
		NoDocumentsFoundInDDEI,
		Completed,
		Failed,
		UnableToEvaluateExistingDocuments
	}
}

