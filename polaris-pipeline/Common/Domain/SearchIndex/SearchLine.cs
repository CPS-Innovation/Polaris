using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json;

namespace Common.Domain.SearchIndex;

public class SearchLine : Line
{
    public SearchLine(string id, long caseId, string documentId, long versionId, string blobName, int pageIndex, int lineIndex, string language, IList<double?> boundingBox, Appearance appearance, 
        string text, IList<Word> words, double pageHeight, double pageWidth)
    {
        Id = id;
        CaseId = caseId;
        DocumentId = documentId;
        VersionId = versionId;
        FileName = blobName;
        PageIndex = pageIndex;
        LineIndex = lineIndex;
        PageHeight = pageHeight;
        PageWidth = pageWidth;

        Language = language;
        BoundingBox = boundingBox;
        Appearance = appearance;
        Text = text;
        Words = words;
    }
    
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("caseId")]
    public long CaseId { get; set; }
    
    [JsonProperty("documentId")]
    public string DocumentId { get; set; }
    
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
}
