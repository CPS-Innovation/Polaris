using Newtonsoft.Json;

namespace Common.Domain.Extensions
{
    public static class JsonExtensions
    {
        public static string ToJson(this object obj)
        {
            return obj == null ? string.Empty : JsonConvert.SerializeObject(obj);
        }
    }
}