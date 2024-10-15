using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Common.Domain.SearchIndex;

public class StreamlinedSearchLine
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("documentId")]
    public string DocumentId
    {
        get
        {
            var base64EncodedBytes = Convert.FromBase64String(Id);
            var indexerId = Encoding.UTF8.GetString(base64EncodedBytes);
            try
            {
                return indexerId.Split(":")[1];
            }
            catch
            {
                return string.Empty;
            }
        }
    }
    [JsonProperty("versionId")]
    public long VersionId { get; set; }
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
