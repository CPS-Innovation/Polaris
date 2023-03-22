namespace Common.Domain.Pipeline
{
    public enum DocumentStatus
	{
		New,
        Indexed,
        PdfUploadedToBlob,
		UnableToConvertToPdf,
		UnexpectedFailure,
		OcrAndIndexFailure,
		DocumentAlreadyProcessed,
	}
}

