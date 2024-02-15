using System;
using System.Linq;
using System.Net.Http.Headers;
using Common.Constants;
using Common.Domain.Document;
using Common.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Common.Extensions
{
    public static class HttpRequestHeadersExtensions
    {
        public static Guid GetCorrelationId(this HttpRequestHeaders headers)
        {
            headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
            if (correlationIdValues == null)
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(headers));

            var correlationId = correlationIdValues.First();
            if (!Guid.TryParse(correlationId, out var currentCorrelationId) || currentCorrelationId == Guid.Empty)
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", correlationId);

            return currentCorrelationId;
        }

        public static Guid GetCorrelation(this IHeaderDictionary headers)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            if (!headers.TryGetValue(HttpHeaderKeys.CorrelationId, out var value))
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(headers));

            if (!Guid.TryParse(value[0], out var correlationId) || correlationId == Guid.Empty)
                throw new BadRequestException("Invalid correlationId. A valid GUID is required.", value);

            return correlationId;
        }

        public static void CheckForCmsAuthValues(this IHeaderDictionary headers)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            if (!headers.TryGetValue(HttpHeaderKeys.CmsAuthValues, out var value))
                throw new BadRequestException("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.", nameof(headers));

            if (string.IsNullOrWhiteSpace(value[0]))
                throw new BadRequestException("Invalid Cms Auth token. A valid Cms Auth token must be received for this request.", value);
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

        public static string GetCaseId(this IHeaderDictionary headers)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            if (!headers.TryGetValue(HttpHeaderKeys.CaseId, out var value))
                throw new BadRequestException("Missing CaseIds", nameof(headers));

            var caseId = value[0];
            if (string.IsNullOrEmpty(caseId))
                throw new BadRequestException("Invalid CaseId", caseId);

            return caseId;
        }

        public static string GetDocumentId(this IHeaderDictionary headers)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            if (!headers.TryGetValue(HttpHeaderKeys.DocumentId, out var value))
                throw new BadRequestException("Missing DocumentIds", nameof(headers));

            var documentId = value[0];
            if (string.IsNullOrEmpty(documentId))
                throw new BadRequestException("Invalid DocumentId", documentId);

            return documentId;
        }

        public static string GetVersionId(this IHeaderDictionary headers)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            if (!headers.TryGetValue(HttpHeaderKeys.VersionId, out var value))
                throw new BadRequestException("Missing VersionIds", nameof(headers));

            var versionId = value[0];
            if (string.IsNullOrEmpty(versionId))
                throw new BadRequestException("Invalid VersionId", versionId);

            return versionId;
        }
    }
}