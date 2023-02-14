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
    public class GeneratePdfHttpRequestFactory : IGeneratePdfHttpRequestFactory
    {
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeneratePdfHttpRequestFactory> _logger;

        public GeneratePdfHttpRequestFactory(IJsonConvertWrapper jsonConvertWrapper, IConfiguration configuration,
            ILogger<GeneratePdfHttpRequestFactory> logger)
        {
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public DurableHttpRequest Create(string caseUrn, long caseId, string documentCategory, string documentId, string fileName, long versionId,
            string cmsAuthValues, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(Create), $"CaseUrn: {caseUrn}, CaseId: {caseId}, DocumentId: {documentId}, VersionId: {versionId}, " +
                                                                  $"FileName: {fileName}");

            try
            {
                var headers = new Dictionary<string, StringValues>
                {
                    { HttpHeaderKeys.ContentType, HttpHeaderValues.ApplicationJson },
                    { HttpHeaderKeys.CorrelationId, correlationId.ToString() },
                    { HttpHeaderKeys.CmsAuthValues, cmsAuthValues }
                };
                var content = _jsonConvertWrapper.SerializeObject(new GeneratePdfRequest(caseUrn, caseId, documentCategory, documentId, fileName, versionId));

                return new DurableHttpRequest(HttpMethod.Post, new Uri(_configuration[ConfigKeys.CoordinatorKeys.PdfGeneratorUrl]), headers, content);
            }
            catch (Exception ex)
            {
                throw new GeneratePdfHttpRequestFactoryException(ex.Message);
            }
            finally
            {
                _logger.LogMethodExit(correlationId, nameof(Create), string.Empty);
            }
        }
    }
}

