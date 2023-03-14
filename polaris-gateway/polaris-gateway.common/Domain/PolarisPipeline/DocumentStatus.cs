namespace PolarisGateway.Domain.PolarisPipeline
{
	public enum DocumentStatus
	{
		None,
		PdfUploadedToBlob,
		Indexed,
		DocumentsRetrieved,
		NotFoundInDDEI,
		UnableToConvertToPdf,
		UnexpectedFailure,
		OcrAndIndexFailure,
		DocumentAlreadyProcessed,
		DocumentEvaluated,
		DocumentRemovedFromSearchIndex,
		UnexpectedSearchIndexRemovalFailure,
		SearchIndexUpdateFailure,
		UnableToEvaluateDocument
	}
}

