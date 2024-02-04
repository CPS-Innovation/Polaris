using System;
using System.Linq;
using System.Net.Http.Headers;
using Common.Constants;
using Common.Domain.Document;
using Common.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Common.Extensions
{
    // todo: DRY on header extraction and processing
    public static class HttpRequestHeadersExtensions
    {
        private const string CorrelationErrorMessage = $"Invalid {HttpHeaderKeys.CorrelationId} header. A valid GUID is required.";

        private const string CmsAuthValuesErrorMessage = $"A valid {HttpHeaderKeys.CmsAuthValues} header is required.";

        public static Guid GetCorrelationId(this HttpRequestHeaders headers)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            if (!headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var values))
                throw new BadRequestException(CorrelationErrorMessage, nameof(headers));

            var firstValue = values.First();
            if (!Guid.TryParse(firstValue, out var value) || value == Guid.Empty)
                throw new BadRequestException(CorrelationErrorMessage, firstValue);

            return value;
        }

        public static Guid GetCorrelationId(this IHeaderDictionary headers)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            if (!headers.TryGetValue(HttpHeaderKeys.CorrelationId, out var values))
                throw new BadRequestException(CorrelationErrorMessage, nameof(headers));

            var firstValue = values.First();
            if (!Guid.TryParse(firstValue, out var value) || value == Guid.Empty)
                throw new BadRequestException(CorrelationErrorMessage, firstValue);

            return value;
        }

        public static string GetCmsAuthValues(this HttpRequestHeaders headers)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            if (!headers.TryGetValues(HttpHeaderKeys.CmsAuthValues, out var values))
                throw new BadRequestException(CmsAuthValuesErrorMessage, nameof(headers));

            var value = values.First();
            if (string.IsNullOrWhiteSpace(value))
                throw new BadRequestException(CmsAuthValuesErrorMessage, value);

            return value;
        }

        public static FileType GetFileType(this IHeaderDictionary headers)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            if (!headers.TryGetValue(HttpHeaderKeys.Filetype, out var value))
                throw new BadRequestException("Missing Filetype Value", nameof(headers));

            var filetypeValue = value[0];
            if (string.IsNullOrEmpty(filetypeValue))
                throw new BadRequestException("Null Filetype Value", filetypeValue);
            if (!Enum.TryParse(filetypeValue, true, out FileType filetype))
                throw new BadRequestException("Invalid Filetype Enum Value", filetypeValue);

            return filetype;
        }
    }
}