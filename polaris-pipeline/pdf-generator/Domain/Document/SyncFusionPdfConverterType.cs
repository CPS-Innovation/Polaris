using pdf_generator.Attributes;

namespace pdf_generator.Domain.Document;

public enum SyncFusionPdfConverterType
{
  [EnumValue("Syncfusion.DocIO")]
  SyncFusionDocIO,
  [EnumValue("Syncfusion.Html")]
  SyncFusionHtml,
  [EnumValue("Syncfusion.Slides")]
  SyncFusionSlides,
  [EnumValue("Syncfusion.Imaging")]
  SyncFusionImaging,
  [EnumValue("Syncfusion.Cells")]
  SyncFusionCell,
  [EnumValue("Syncfusion.Xps")]
  SyncFusionXps,
  [EnumValue("Syncfusion.Pdf")]
  SyncFusionPdf,
  [EnumValue("Converter not used")]
  None
}