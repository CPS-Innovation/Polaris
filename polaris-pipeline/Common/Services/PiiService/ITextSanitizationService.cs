
namespace Common.Services.PiiService
{
  public interface ITextSanitizationService
  {
    string SantitizeText(string text);
  }
}