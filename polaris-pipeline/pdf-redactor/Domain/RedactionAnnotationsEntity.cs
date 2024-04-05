

namespace pdf_redactor.Domain
{
  public class RedactionAnnotationsEntity
  {
    public int PageNumber { get; set; }
    public double ImageConversionDurationMilliseconds { get; set; }
    public double TotalFinaliseAnnotationsDurationMilliseconds { get; set; }
    public double DeletePageDurationMilliseconds { get; set; }
    public double InsertPageDurationMilliseconds { get; set; }
  }
}