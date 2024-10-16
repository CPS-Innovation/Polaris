namespace Common.Services.PiiService
{
  public class TextSanitizationService : ITextSanitizationService
  {
    private static readonly char[] ASCII_CHARS = new char[]
    {
      '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/', ':', ';', '<', '=', '>', '?', '@', '[', '\\', ']', '^', '_', '`', '{', '|', '}', '~'
    };

    public string SantitizeText(string text)
    {
      if (string.IsNullOrEmpty(text))
      {
        return text;
      }

      return text.TrimStart(ASCII_CHARS).TrimEnd(ASCII_CHARS);
    }
  }
}
