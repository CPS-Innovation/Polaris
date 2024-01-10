using Newtonsoft.Json;

namespace polaris_common.Domain.Extensions
{
    public static class JsonExtensions
    {
        public static string ToJson(this object obj)
        {
            return obj == null ? string.Empty : JsonConvert.SerializeObject(obj);
        }
    }
}