namespace Common.Domain.Pipeline
{
    public enum DocumentStatus
	{
		None,
        Indexed,
        PdfUploadedToBlob,
		UnableToConvertToPdf,
		UnexpectedFailure,
		OcrAndIndexFailure,
		DocumentAlreadyProcessed,
	}
}

