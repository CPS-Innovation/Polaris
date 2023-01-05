using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Adapters;
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
        private readonly IIdentityClientAdapter _identityClientAdapter;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TextExtractorHttpRequestFactory> _logger;

        public TextExtractorHttpRequestFactory(IIdentityClientAdapter identityClientAdapter,
            IJsonConvertWrapper jsonConvertWrapper, IConfiguration configuration, ILogger<TextExtractorHttpRequestFactory> logger)
		{
            _identityClientAdapter = identityClientAdapter ?? throw new ArgumentNullException(nameof(identityClientAdapter));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<DurableHttpRequest> Create(long caseId, string documentId, long versionId, string blobName, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(Create), $"CaseId: {caseId}, DocumentId: {documentId}, BlobName: {blobName}");

            try
            {
                var clientScopes = _configuration[ConfigKeys.CoordinatorKeys.TextExtractorScope];

                _logger.LogMethodFlow(correlationId, nameof(Create), $"Getting client access token for '{clientScopes}'");
                var result = await _identityClientAdapter.GetClientAccessTokenAsync(clientScopes, correlationId);

                var headers = new Dictionary<string, StringValues>
                {
                    { HttpHeaderKeys.ContentType, HttpHeaderValues.ApplicationJson },
                    { HttpHeaderKeys.Authorization, $"{HttpHeaderValues.AuthTokenType} {result}"},
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

