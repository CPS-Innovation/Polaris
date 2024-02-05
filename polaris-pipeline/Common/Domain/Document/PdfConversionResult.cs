using System.IO;
using Common.Domain.Extensions;

namespace Common.Domain.Document;

public class PdfConversionResult
{
    public PdfConversionResult(string documentId, PdfConverterType conversionHandler)
    {
        DocumentId = documentId;
        ConversionHandler = conversionHandler;
    }
    
    public string DocumentId { get; set; }
    
    public Stream ConvertedDocument { get; set; }

    public PdfConverterType ConversionHandler { get; set; }
    
    public PdfConversionStatus ConversionStatus { get; set; }

    public string Feedback { get; set; }

    public void RecordConversionSuccess(Stream convertedDocument)
    {
        ConvertedDocument = convertedDocument;
        ConversionStatus = PdfConversionStatus.DocumentConverted;
    }

    public void RecordConversionFailure(PdfConversionStatus status, string message)
    {
        ConversionStatus = status;
        Feedback = $"Unable to convert document with id '{DocumentId}' to pdf. Exception: {message}.";
    }

    public string GetFailureReason()
    {
        return $"{ConversionHandler.GetEnumValue()} - {ConversionStatus.GetEnumValue()}: {Feedback}";
    }
}