namespace PolarisGateway.Domain.PolarisPipeline
{
	public enum DocumentStatus
	{
		None,
		PdfUploadedToBlob,
		Indexed,
		// ReSharper disable once InconsistentNaming
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

