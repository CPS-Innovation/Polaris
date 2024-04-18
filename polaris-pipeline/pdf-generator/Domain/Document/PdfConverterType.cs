using pdf_generator.Attributes;

namespace pdf_generator.Domain.Document;

public enum PdfConverterType
{
    [EnumValue("Aspose PDF")]
    AsposePdf,
    [EnumValue("Aspose Words")]
    AsposeWords,
    [EnumValue("Aspose Cells")]
    AsposeCells,
    [EnumValue("Aspose Slides")]
    AsposeSlides,
    [EnumValue("Aspose Imaging")]
    AsposeImaging,
    [EnumValue("Aspose Diagrams")]
    AsposeDiagrams,
    [EnumValue("Aspose Email")]
    AsposeEmail,
    [EnumValue("Converter not used")]
    None
}