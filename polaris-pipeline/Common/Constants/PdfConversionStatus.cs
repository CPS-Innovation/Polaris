﻿using Common.Attributes;

namespace Common.Constants;

public enum PdfConversionStatus
{
    [EnumValue("Document converted")]
    DocumentConverted,
    [EnumValue("Pdf is encrypted")]
    PdfEncrypted,
    [EnumValue("Document type not supported by Polaris")]
    DocumentTypeUnsupported,

    [EnumValue("File is password protected")]
    AsposePdfPasswordProtected,
    [EnumValue("Invalid File Format")]
    AsposePdfInvalidFileFormat,
    [EnumValue("PDF is corrupted or structurally invalid")]
    AsposePdfException,

    [EnumValue("Unsupported file format")]
    AsposeWordsUnsupportedFileFormat,
    [EnumValue("File is password protected")]
    AsposeWordsPasswordProtected,

    [EnumValue("Unsupported file format")]
    AsposeHtmlUnsupportedFileFormat,
    [EnumValue("File is password protected")]
    AsposeHtmlPasswordProtected,

    [EnumValue("General cells library error")]
    AsposeCellsGeneralError,

    [EnumValue("Cannot load image")]
    AsposeImagingCannotLoad,

    [EnumValue("An unexpected error occurred")]
    UnexpectedError,
    [EnumValue("Slides file is password protected")]
    AsposeSlidesPasswordProtected,
}