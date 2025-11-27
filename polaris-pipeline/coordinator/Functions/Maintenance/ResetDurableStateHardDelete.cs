using System.Threading.Tasks;
using DurableTask.AzureStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;

namespace coordinator.Functions.Maintenance;

// See https://github.com/CPS-Innovation/Polaris/blob/d770812285c46b214d9f65fad45204e149113a88/polaris-pipeline/coordinator/Functions/Orchestration/Functions/Maintenance/ResetDurableState.cs#L13
//  for the original implementation of an overnight clear down based on time range

// This code taken from Azure functions durable extension discussion [0]. As per comment [1]
//  "this is very much a non-graceful purge of all orchestration data."
//
// [0]: https://github.com/Azure/azure-functions-durable-extension/discussions/2029
// [1]: https://github.com/Azure/azure-functions-durable-extension/discussions/2029#discussioncomment-1760004
public class ResetDurableStateHardDelete
{
    readonly IConfiguration _configuration;

    // INameResolver is a service of the Functions host that can
    // be used to look up app settings.
    public ResetDurableStateHardDelete(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Function(nameof(ResetDurableStateHardDelete))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetDurableState(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        [DurableClient] DurableTaskClient client)
    {
        var settings = new AzureStorageOrchestrationServiceSettings
        {
            TaskHubName = _configuration.GetValue<string>("CoordinatorTaskHub"),
        }; 
        var connectionString = _configuration.GetValue<string>("Storage");
        settings.StorageAccountClientProvider = new StorageAccountClientProvider(connectionString);
        var storageService = new AzureStorageOrchestrationService(settings);
        
        await storageService.DeleteAsync();

        return new OkResult();
    }
}