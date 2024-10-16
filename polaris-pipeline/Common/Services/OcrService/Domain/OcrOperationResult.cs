
namespace Common.Services.OcrService.Domain
{
  public class OcrOperationResult
  {
    public bool IsSuccess { get; set; }

    public AnalyzeResults AnalyzeResults { get; set; }
  }
}