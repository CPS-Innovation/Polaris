using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Logging;
using DurableTask.AzureStorage;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.Orchestration.Functions.Maintenance;

public class ResetDurableState
{
    private readonly ILogger<ResetDurableState> _logger;
    private readonly IConfiguration _configuration;

    private const string LoggingName = $"{nameof(ResetDurableState)} - {nameof(RunAsync)}";

    public ResetDurableState(ILogger<ResetDurableState> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    [FunctionName("ResetDurableState")]
    public async Task<HttpResponseMessage> RunAsync([HttpTrigger(AuthorizationLevel.Function, "delete", 
        Route = RestApi.ResetDurableState)] HttpRequestMessage req, [DurableClient] IDurableClient client)
    {
        try
        {
            var correlationId = Guid.NewGuid();
            _logger.LogMethodEntry(correlationId, LoggingName, string.Empty);
            
            var connectionString = _configuration[ConfigKeys.CoordinatorKeys.AzureWebJobsStorage];
            var settings = new AzureStorageOrchestrationServiceSettings
            {
                StorageConnectionString = connectionString,
                TaskHubName = client.TaskHubName
            };
            
            var storageService = new AzureStorageOrchestrationService(settings);

            // Delete all Azure Storage tables, blobs, and queues in the task hub
            _logger.LogMethodFlow(correlationId, LoggingName, $"Deleting all storage resources for task hub {settings.TaskHubName}...");
            await storageService.DeleteAsync();
            
            // Wait for a minute to allow Azure Storage time to reset itself, otherwise it won't recreate resources with the same names as before.
            _logger.LogMethodFlow(correlationId, LoggingName, "The delete operation completed. Waiting one minute before recreating...");
            await Task.Delay(TimeSpan.FromMinutes(1));
            
            // Optional: Recreate all the Azure Storage resources for this task hub. This happens automatically whenever the function app restarts,
            // so it's not a required step, but it will slightly improve first-user performance via faster warm-up times
            _logger.LogMethodFlow(correlationId, LoggingName, $"Recreating storage resources for task hub {settings.TaskHubName}...");
            await storageService.CreateIfNotExistsAsync();
            
            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return req.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}
