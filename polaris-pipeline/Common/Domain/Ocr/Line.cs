using System.Collections.Generic;
using Newtonsoft.Json;

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

    [JsonProperty(PropertyName = "boundingBox")]
    public IList<double?> BoundingBox { get; set; }

    [JsonProperty(PropertyName = "text")]
    public string Text { get; set; }

    [JsonProperty(PropertyName = "words")]
    public IList<Word> Words { get; set; }

  }
}