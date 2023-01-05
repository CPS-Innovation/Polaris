using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Adapters;
using Common.Constants;
using Common.Domain.DocumentEvaluation;
using Common.Domain.Requests;
using Common.Logging;
using Common.Wrappers;
using coordinator.Domain.Exceptions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace coordinator.Factories;

public class DocumentEvaluationHttpRequestFactory : IDocumentEvaluationHttpRequestFactory
{
    private readonly IIdentityClientAdapter _identityClientAdapter;
    private readonly IJsonConvertWrapper _jsonConvertWrapper;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DocumentEvaluationHttpRequestFactory> _logger;

    public DocumentEvaluationHttpRequestFactory(IIdentityClientAdapter identityClientAdapter, IJsonConvertWrapper jsonConvertWrapper, IConfiguration configuration, 
        ILogger<DocumentEvaluationHttpRequestFactory> logger)
    {
        _identityClientAdapter = identityClientAdapter ?? throw new ArgumentNullException(nameof(identityClientAdapter));
        _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));;
    }
    
    public async Task<DurableHttpRequest> Create(string caseUrn, long caseId, List<DocumentToRemove> documentsToRemove, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(Create), $"CaseUrn: {caseUrn}, CaseId: {caseId}");
        
        try
        {
            var clientScopes = _configuration[ConfigKeys.CoordinatorKeys.DocumentEvaluatorScope];
                
            var result = await _identityClientAdapter.GetClientAccessTokenAsync(clientScopes, correlationId);
                
            var headers = new Dictionary<string, StringValues>
            {
                { HttpHeaderKeys.ContentType, HttpHeaderValues.ApplicationJson },
                { HttpHeaderKeys.Authorization, $"{HttpHeaderValues.AuthTokenType} {result}"},
                { HttpHeaderKeys.CorrelationId, correlationId.ToString() }
            };
            var content = _jsonConvertWrapper.SerializeObject(new ProcessDocumentsToRemoveRequest(caseUrn, caseId, documentsToRemove));

            return new DurableHttpRequest(HttpMethod.Post, new Uri(_configuration[ConfigKeys.CoordinatorKeys.DocumentEvaluatorUrl]), headers, content);
        }
        catch(Exception ex)
        {
            throw new GeneratePdfHttpRequestFactoryException(ex.Message);
        }
        finally
        {
            _logger.LogMethodExit(correlationId, nameof(Create), string.Empty);
        }
    }
}
