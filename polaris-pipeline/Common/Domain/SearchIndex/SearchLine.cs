using System;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json;

namespace Common.Domain.SearchIndex;

public class SearchLine : Line
{
        public SearchLine(  string id,
                            Guid polarisDocumentId, 
                            string blobName, 
                            int pageIndex, 
                            int lineIndex, 
                            string language, 
                            IList<double?> boundingBox, 
                            Appearance appearance, 
                            string text, 
                            IList<Word> words, 
                            double pageHeight, 
                            double pageWidth
                        )
    {
        Id = id;
        PolarisDocumentId = polarisDocumentId;
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

    [JsonProperty("polarisDocumentId")]
    public Guid PolarisDocumentId { get; set; }

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
