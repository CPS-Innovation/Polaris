namespace PolarisGateway.Domain.PolarisPipeline
{
	public enum DocumentStatus
	{
		None,
		PdfUploadedToBlob,
		Indexed,
		NotFoundInDDEI,
		UnableToConvertToPdf,
		UnexpectedFailure,
		OcrAndIndexFailure,
		DocumentAlreadyProcessed,
	}
}

