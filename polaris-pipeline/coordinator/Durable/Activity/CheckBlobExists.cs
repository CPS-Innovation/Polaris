using System.Threading.Tasks;
using Common.Services.BlobStorage;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Durable.Activity
{
    public class CheckBlobExists
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;

        public CheckBlobExists(IPolarisBlobStorageService polarisBlobStorageService)
        {
            _polarisBlobStorageService = polarisBlobStorageService;
        }

        [FunctionName(nameof(CheckBlobExists))]
        public async Task<bool> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var (blobId, isOcred) = context.GetInput<(BlobIdType, bool)>();

            return await _polarisBlobStorageService.BlobExistsAsync(blobId, isOcred);
        }
    }
}
