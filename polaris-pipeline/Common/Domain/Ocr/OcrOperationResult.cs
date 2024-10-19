
namespace Common.Domain.Ocr
{
  public class OcrOperationResult
  {
    public bool IsSuccess { get; set; }

    public AnalyzeResults AnalyzeResults { get; set; }
  }
}