using Common.Wrappers.Contracts;
using Newtonsoft.Json;
using System;

namespace Common.Wrappers
{
    public class JsonConvertWrapper : IJsonConvertWrapper
    {
        public JsonConvertWrapper()
        {
        }

        public string SerializeObject(object objectToSerialize)
        {
            return JsonConvert.SerializeObject(objectToSerialize, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        public T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

        public string SerializeObject(object objectToSerialize, Guid correlationId) =>
            JsonConvert.SerializeObject(objectToSerialize, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

        public T DeserializeObject<T>(string value, Guid correlationId) =>
            JsonConvert.DeserializeObject<T>(value);
    }
}
