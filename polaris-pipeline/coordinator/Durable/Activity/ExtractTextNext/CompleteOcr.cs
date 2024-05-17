using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common.Services.BlobStorageService;
using Common.Wrappers;
using coordinator.Services.OcrService;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Durable.Activity.ExtractTextNext
{
    public class CompleteOcr
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IOcrService _ocrService;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public CompleteOcr(IPolarisBlobStorageService blobStorageService, IOcrService ocrService, IJsonConvertWrapper jsonConvertWrapper)
        {
            _blobStorageService = blobStorageService;
            _ocrService = ocrService;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        [FunctionName(nameof(CompleteOcr))]
        public async Task<bool> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var (operationId, ocrBlobName, correlationId) = context.GetInput<(Guid, string, Guid)>();
            var (isOperationComplete, analyzeResult) = await _ocrService.GetOperationResultsAsync(operationId, correlationId);

            if (!isOperationComplete)
            {
                return false;
            }

            var jsonResults = _jsonConvertWrapper.SerializeObject(analyzeResult);
            using var ocrStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonResults));

            await _blobStorageService.UploadDocumentAsync(
                ocrStream,
                ocrBlobName);

            return true;
        }
    }
}
