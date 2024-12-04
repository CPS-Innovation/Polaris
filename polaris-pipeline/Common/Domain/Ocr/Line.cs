using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common.Domain.Ocr
{
  public class Line
  {
    public Line(IList<double?> boundingBox, string text, IList<Word> words)
    {
      BoundingBox = boundingBox;
      Text = text;
      Words = words;
    }

    [JsonPropertyName("boundingBox")]
    public IList<double?> BoundingBox { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("words")]
    public IList<Word> Words { get; set; }

  }
}