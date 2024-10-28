namespace Common.Services.PiiService.TextSanitization
{
  public class TextSanitizationService : ITextSanitizationService
  {
    private static readonly char[] ASCII_CHARS = new char[]
    {
      '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '_', '`', '{', '|', '}', '~'
    };

    public string SanitizeText(string text)
    {
      if (string.IsNullOrEmpty(text))
      {
        return text;
      }

      return text.TrimStart(ASCII_CHARS).TrimEnd(ASCII_CHARS);
    }
  }
}
