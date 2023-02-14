using System;
using System.Collections.Generic;
using System.Net.Http;
using Common.Constants;
using Common.Domain.Requests;
using Common.Logging;
using Common.Wrappers;
using coordinator.Domain.Exceptions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace coordinator.Factories
{
	public class TextExtractorHttpRequestFactory : ITextExtractorHttpRequestFactory
    {
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TextExtractorHttpRequestFactory> _logger;

        public TextExtractorHttpRequestFactory(IJsonConvertWrapper jsonConvertWrapper, IConfiguration configuration, ILogger<TextExtractorHttpRequestFactory> logger)
		{
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public DurableHttpRequest Create(long caseId, string documentId, long versionId, string blobName, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(Create), $"CaseId: {caseId}, DocumentId: {documentId}, BlobName: {blobName}");

            try
            {
                var headers = new Dictionary<string, StringValues>
                {
                    { HttpHeaderKeys.ContentType, HttpHeaderValues.ApplicationJson },
                    { HttpHeaderKeys.CorrelationId, correlationId.ToString() }
                };
                var content = _jsonConvertWrapper.SerializeObject(new ExtractTextRequest(caseId, documentId, versionId, blobName));

                return new DurableHttpRequest(HttpMethod.Post, new Uri(_configuration[ConfigKeys.CoordinatorKeys.TextExtractorUrl]), headers,
                    content);
            }
            catch (Exception ex)
            {
                throw new TextExtractorHttpRequestFactoryException(ex.Message);
            }
            finally
            {
                _logger.LogMethodExit(correlationId, nameof(Create), string.Empty);
            }
        }
	}
}

