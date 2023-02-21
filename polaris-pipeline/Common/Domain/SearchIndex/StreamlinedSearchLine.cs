using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Domain.SearchIndex;

public class StreamlinedSearchLine
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("fileName")]
    public string FileName { get; set; }

    [JsonProperty("pageIndex")]
    public int PageIndex { get; set; }

    [JsonProperty("lineIndex")]
    public int LineIndex { get; set; }

    [JsonProperty("pageHeight")]
    public double PageHeight { get; set; }

    [JsonProperty("pageWidth")]
    public double PageWidth { get; set; }

    [JsonProperty(PropertyName = "text")]
    public string Text { get; set; }

    [JsonProperty(PropertyName = "words")]
    public IList<StreamlinedWord> Words { get; set; }
}
