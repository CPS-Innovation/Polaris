
namespace Common.Services.PiiService.TextSanitization
{
  public interface ITextSanitizationService
  {
    string SanitizeText(string text);
  }
}