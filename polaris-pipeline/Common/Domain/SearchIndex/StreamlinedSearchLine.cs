using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Common.Domain.SearchIndex;

public class StreamlinedSearchLine
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("documentId")]
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
    [JsonPropertyName("versionId")]
    public long VersionId { get; set; }
    [JsonPropertyName("fileName")]
    public string FileName { get; set; }

    [JsonPropertyName("pageIndex")]
    public int PageIndex { get; set; }

    [JsonPropertyName("lineIndex")]
    public int LineIndex { get; set; }

    [JsonPropertyName("pageHeight")]
    public double PageHeight { get; set; }

    [JsonPropertyName("pageWidth")]
    public double PageWidth { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("words")]
    public IList<StreamlinedWord> Words { get; set; }
}
