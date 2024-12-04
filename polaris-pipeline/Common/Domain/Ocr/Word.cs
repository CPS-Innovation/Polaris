using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common.Domain.Ocr;

public class Word
{
    public Word()
    {
    }

    public Word(IList<double?> boundingBox, string text, double confidence)
    {
        BoundingBox = boundingBox;
        Text = text;
        Confidence = confidence;
    }

    [JsonPropertyName("boundingBox")]
    public IList<double?> BoundingBox { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}