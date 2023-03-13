namespace coordinator.Domain.Tracker
{
    public enum DocumentStatus
    {
        None,
        PdfUploadedToBlob,
        Indexed,
        NotFoundInDDEI,
        UnableToConvertToPdf,
        UnexpectedFailure,
        // todo: smell - there are probably two independent variables described
        //  in one here.
        OcrAndIndexFailure,
        // todo: this status should not exist
        DocumentAlreadyProcessed
    }
}

