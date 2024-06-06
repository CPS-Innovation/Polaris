using System.Linq;
using System.Text;

namespace coordinator.Services.TextSanitizationService
{
  public class TextSanitizationService : ITextSanitizationService
  {
    private static readonly char[] ASCII_CHARS = new char[] { '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '_', '`', '{', '|', '}', '~' };

    public string SantitizeText(string text)
    {
      StringBuilder sanitizedText = new StringBuilder();
      foreach (char c in text)
      {
        if (!ASCII_CHARS.Contains(c))
        {
          sanitizedText.Append(c);
        }
      }
      return sanitizedText.ToString();
    }
  }
}
