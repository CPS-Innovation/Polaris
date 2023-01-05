using Newtonsoft.Json;

namespace RumpoleGateway.Extensions;

public static class JsonExtensions
{
    public static string ToJson(this object obj)
    {
        return obj == null ? string.Empty : JsonConvert.SerializeObject(obj);
    }
}