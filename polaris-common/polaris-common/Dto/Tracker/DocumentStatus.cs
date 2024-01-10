namespace polaris_common.Dto.Tracker
{
    public enum DocumentStatus
    {
        New,
        Indexed,
        PdfUploadedToBlob,
        UnableToConvertToPdf,
        OcrAndIndexFailure,
        DocumentAlreadyProcessed,
    }
}