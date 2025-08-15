using System;

namespace Common.Exceptions;

public class OcrDocumentNotFoundException : Exception
{
    public OcrDocumentNotFoundException() : base("Ocr Document not found, please generate OCR document")
    {
    }
}