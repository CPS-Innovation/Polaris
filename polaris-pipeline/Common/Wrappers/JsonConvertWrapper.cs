using Newtonsoft.Json;

namespace Common.Wrappers
{
    public class JsonConvertWrapper : IJsonConvertWrapper
    {
        public JsonConvertWrapper()
        {
        }

        public string SerializeObject(object objectToSerialize) =>
            JsonConvert.SerializeObject(objectToSerialize, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

        public T DeserializeObject<T>(string value) =>
            JsonConvert.DeserializeObject<T>(value);
    }
}
