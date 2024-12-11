using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Domain.Ocr
{
  public class Word
  {
    public Word(IList<double?> boundingBox, string text, double confidence)
    {
      BoundingBox = boundingBox;
      Text = text;
      Confidence = confidence;
    }

    [JsonProperty(PropertyName = "boundingBox")]
    public IList<double?> BoundingBox { get; set; }

    [JsonProperty(PropertyName = "text")]
    public string Text { get; set; }

    [JsonProperty(PropertyName = "confidence")]
    public double Confidence { get; set; }
  }
}