using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RumpoleGateway.Domain.Logging;

namespace RumpoleGateway.Wrappers
{
    public class JsonConvertWrapper : IJsonConvertWrapper
    {
        private readonly ILogger<JsonConvertWrapper> _logger;

        public JsonConvertWrapper(ILogger<JsonConvertWrapper> logger)
        {
            _logger = logger;
        }

        public string SerializeObject(object objectToSerialize, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(SerializeObject), string.Empty);
            var result = JsonConvert.SerializeObject(objectToSerialize, Formatting.None, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            _logger.LogMethodExit(correlationId, nameof(SerializeObject), string.Empty);
            return result;
        }

        public T DeserializeObject<T>(string value, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(DeserializeObject), string.Empty);
            var result = JsonConvert.DeserializeObject<T>(value);
            _logger.LogMethodExit(correlationId, nameof(DeserializeObject), string.Empty);
            return result;
        }
    }
}
