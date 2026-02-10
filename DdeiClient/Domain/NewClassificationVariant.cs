using System.Text.Json.Serialization;

namespace DdeiClient.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NewClassificationVariant
{
    Statement,
    Exhibit,
    Immediate,
    Other
}