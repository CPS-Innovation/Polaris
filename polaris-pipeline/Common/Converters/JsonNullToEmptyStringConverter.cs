using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Converters;

public class JsonNullToEmptyStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(string.IsNullOrEmpty(value) ? null : value);
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(string);
    }
}