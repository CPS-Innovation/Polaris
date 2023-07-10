using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Common.Constants;
using Common.Domain.Exceptions;
using Newtonsoft.Json;

namespace Common.Extensions
{
    public static class HttpRequestHeadersExtensions
    {
        public static Guid GetCorrelationId(this HttpRequestHeaders headers)
        {
            Guid currentCorrelationId = default;

            headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
            if (correlationIdValues == null)
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(headers));

            var correlationId = correlationIdValues.First();
            if (!Guid.TryParse(correlationId, out currentCorrelationId) || currentCorrelationId == Guid.Empty)
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.",
                    correlationId);

            return currentCorrelationId;
        }

        public static T ToDto<T>(this HttpHeaders headers)
        {
            var dict = ToDictionary(headers);
            var interimJson = JsonConvert.SerializeObject(dict);
            return JsonConvert.DeserializeObject<T>(interimJson);
        }

        private static Dictionary<string, string> ToDictionary(this HttpHeaders headers)
        {
            var dict = new Dictionary<string, string>();

            foreach (var item in headers.ToList())
            {
                if (item.Value != null)
                {
                    var header = String.Empty;
                    foreach (var value in item.Value)
                    {
                        header += value + " ";
                    }

                    // Trim the trailing space and add item to the dictionary
                    header = header.TrimEnd(" ".ToCharArray());
                    dict.Add(item.Key, header);
                }
            }

            return dict;
        }
    }
}