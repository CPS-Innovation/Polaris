using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public static class Json
{

    static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
    {
        Formatting = Formatting.Indented,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
    };
    public static string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, _jsonSerializerSettings);
    }

    public static T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings)!;
    }
}