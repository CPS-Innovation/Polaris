namespace Common.Domain.Case
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

