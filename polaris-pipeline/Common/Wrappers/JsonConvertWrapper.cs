using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Wrappers
{
    public class JsonConvertWrapper : IJsonConvertWrapper
    {
        private readonly JsonSerializerOptions _jsonSerializerSerializeOptions = new () { WriteIndented = false, ReferenceHandler = ReferenceHandler.IgnoreCycles, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private readonly JsonSerializerOptions _jsonSerializerDeserializeOptions = new () { PropertyNameCaseInsensitive = true };

        public JsonConvertWrapper()
        {
        }

        public string SerializeObject(object objectToSerialize) =>
            JsonSerializer.Serialize(objectToSerialize, _jsonSerializerSerializeOptions);

        public T DeserializeObject<T>(string value) => JsonSerializer.Deserialize<T>(value, _jsonSerializerDeserializeOptions);
    }
}
