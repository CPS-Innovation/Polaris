namespace Common.Domain.Case
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

