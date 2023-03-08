namespace PolarisGateway.Domain.PolarisPipeline
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

