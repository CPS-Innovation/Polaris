using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Services.OcrService.Domain
{
  public class Line
  {
    [JsonProperty(PropertyName = "boundingBox")]
    public IList<double?> BoundingBox { get; set; }

    [JsonProperty(PropertyName = "text")]
    public string Text { get; set; }

    [JsonProperty(PropertyName = "words")]
    public IList<Word> Words { get; set; }
  }
}