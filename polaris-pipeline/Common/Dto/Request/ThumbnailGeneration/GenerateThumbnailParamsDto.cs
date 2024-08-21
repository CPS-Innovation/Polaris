using Newtonsoft.Json;

namespace Common.Dto.Request.DocumentManipulation
{
  public class GenerateThumbnailParamsDto
  {
    [JsonProperty("pageIndex")]
    public int? PageIndex { get; set; } = 1;
    [JsonProperty("height")]
    public int? Height { get; set; } = 1000;
    [JsonProperty("width")]
    public int? Width { get; set; } = 1000;
  }
}