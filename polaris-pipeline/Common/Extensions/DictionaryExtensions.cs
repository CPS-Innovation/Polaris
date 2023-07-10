using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static void PopulateFromDto<T>(this Dictionary<string, StringValues> dict, T obj)
        {
            var objJson = JsonConvert.SerializeObject(obj);
            var objDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(objJson);
            foreach (var item in objDict)
            {
                dict.Add(item.Key, item.Value);
            }
        }
    }
}