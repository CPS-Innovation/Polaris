using System.Net.Http;
using System.Threading.Tasks;
using DurableTask.AzureStorage;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace coordinator.Functions.Orchestration.Functions.Maintenance;

// See https://github.com/CPS-Innovation/Polaris/blob/d770812285c46b214d9f65fad45204e149113a88/polaris-pipeline/coordinator/Functions/Orchestration/Functions/Maintenance/ResetDurableState.cs#L13
//  for the original implementation of an overnight clear down based on time range

// This code taken from Azure functions durable extension discussion [0]. As per comment [1]
//  "this is very much a non-graceful purge of all orchestration data."
//
// [0]: https://github.com/Azure/azure-functions-durable-extension/discussions/2029
// [1]: https://github.com/Azure/azure-functions-durable-extension/discussions/2029#discussioncomment-1760004
public class ResetDurableStateHardDelete
{
    readonly INameResolver _nameResolver;

    // INameResolver is a service of the Functions host that can
    // be used to look up app settings.
    public ResetDurableStateHardDelete(INameResolver nameResolver)
    {
        _nameResolver = nameResolver;
    }

    [FunctionName(nameof(ResetDurableStateHardDelete))]
    public async Task<HttpResponseMessage> ResetDurableState(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestMessage req,
        [DurableClient] IDurableClient client)
    {
        var connString = _nameResolver.Resolve("AzureWebJobsStorage");
        var settings = new AzureStorageOrchestrationServiceSettings
        {
            StorageConnectionString = connString,
            TaskHubName = client.TaskHubName,
        };

        var storageService = new AzureStorageOrchestrationService(settings);

        await storageService.DeleteAsync();

        return req.CreateResponse(System.Net.HttpStatusCode.OK);
    }
}
