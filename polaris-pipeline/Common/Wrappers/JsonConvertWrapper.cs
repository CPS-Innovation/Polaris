using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Wrappers
{
    public class JsonConvertWrapper : IJsonConvertWrapper
    {
        public JsonConvertWrapper()
        {
        }

        public string SerializeObject(object objectToSerialize) =>
            JsonSerializer.Serialize(objectToSerialize, new JsonSerializerOptions { WriteIndented = false, ReferenceHandler = ReferenceHandler.IgnoreCycles });

        public T DeserializeObject<T>(string value) =>
            JsonSerializer.Deserialize<T>(value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
