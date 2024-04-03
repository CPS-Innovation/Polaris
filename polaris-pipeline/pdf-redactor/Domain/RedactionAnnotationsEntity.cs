

namespace pdf_redactor.Domain
{
  public class RedactionAnnotationsEntity
  {
    public int PageNumber { get; set; }
    public double ImageConversionDurationSeconds { get; set; }
    public double TotalFinaliseAnnotationsDurationSeconds { get; set; }
    public double DeletePageDurationSeconds { get; set; }
    public double InsertPageDurationSeconds { get; set; }
  }
}