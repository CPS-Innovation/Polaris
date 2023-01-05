using System.Threading.Tasks;
using System;
using System.Linq;
using Azure.Storage.Queues.Models;
using Common.Constants;
using Common.Domain.QueueItems;
using Common.Logging;
using Common.Services.SearchIndexService.Contracts;
using Common.Wrappers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace document_evaluator.Functions;

public class UpdateSearchIndexByBlobName
{
    private readonly IJsonConvertWrapper _jsonConvertWrapper;
    private readonly IValidatorWrapper<UpdateSearchIndexByBlobNameQueueItem> _validatorWrapper;
    private readonly IConfiguration _configuration;
    private readonly ISearchIndexService _searchIndexService;

    public UpdateSearchIndexByBlobName(IJsonConvertWrapper jsonConvertWrapper, IValidatorWrapper<UpdateSearchIndexByBlobNameQueueItem> validatorWrapper, 
        IConfiguration configuration, ISearchIndexService searchIndexService)
    {
        _jsonConvertWrapper = jsonConvertWrapper;
        _validatorWrapper = validatorWrapper;
        _configuration = configuration;
        _searchIndexService = searchIndexService;
    }
    
    [FunctionName("update-search-index-by-blob-name")]
    public async Task RunAsync([QueueTrigger("update-search-index-by-blob-name")] QueueMessage message, ILogger log)
    {
        log.LogInformation("Received message from {QueueName}, content={Content}", _configuration[ConfigKeys.SharedKeys.UpdateSearchIndexByBlobNameQueueName], message.MessageText);
        
        var request = _jsonConvertWrapper.DeserializeObject<UpdateSearchIndexByBlobNameQueueItem>(message.MessageText);
        if (request == null)
            throw new Exception($"An invalid message received on the queue: '{message.MessageText}'");
        
        var results = _validatorWrapper.Validate(request);
        if (results.Any())
            throw new Exception(string.Join(Environment.NewLine, results));
        
        log.LogMethodFlow(request.CorrelationId, nameof(RunAsync), $"Beginning search index update for: {message.MessageText}");

        await _searchIndexService.RemoveResultsByBlobNameAsync(request.CaseId, request.BlobName, request.CorrelationId);
        
        log.LogMethodFlow(request.CorrelationId, nameof(RunAsync), $"Search index update completed for: {message.MessageText}");
    }
}
