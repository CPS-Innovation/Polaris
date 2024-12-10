using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Common.Domain.SearchIndex;

public class SearchLine : Line, ISearchable
{
    public SearchLine()
    {
    }

    public SearchLine(string id, int caseId, string documentId, long versionId, string blobName, int pageIndex, int lineIndex, string language, IList<double?> boundingBox, Appearance appearance,
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

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("caseId")]
    public int CaseId { get; set; }

    [JsonPropertyName("documentId")]
    public string DocumentId { get; set; }

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
}

public class Line
{
    [JsonPropertyName("language")]
    public string Language { get; set; }

    [JsonPropertyName("boundingBox")]
    public IList<double?> BoundingBox { get; set; }

    [JsonPropertyName("appearance")]
    public Appearance Appearance { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("words")]
    public IList<Word> Words { get; set; }

    public Line()
    {
    }

    public Line(IList<double?> boundingBox, string text, IList<Word> words, string language = null, Appearance appearance = null)
    {
        Language = language;
        BoundingBox = boundingBox;
        Appearance = appearance;
        Text = text;
        Words = words;
    }
}