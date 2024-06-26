using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common.Services.BlobStorageService;
using Common.Wrappers;
using coordinator.Services.OcrService;
using coordinator.Services.OcrService.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Durable.Activity
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
        public async Task<(bool, AnalyzeResults)> Run([ActivityTrigger] IDurableActivityContext context)
        {
            // var (operationId, ocrBlobName, correlationId, subCorrelationId) = context.GetInput<(Guid, string, Guid, Guid?)>();
            // var (isOperationComplete, operationResults) = await _ocrService.GetOperationResultsAsync(operationId, correlationId);
            var (operationId, ocrBlobName, correlationId, subCorrelationId) = context.GetInput<(Guid, string, Guid, Guid?)>();
            var (isOperationComplete, analyzeResult) = await _ocrService.GetOperationResultsAsync(operationId, correlationId);

            if (!isOperationComplete)
            {
                return (false, null);
            }

            var jsonResults = _jsonConvertWrapper.SerializeObject(analyzeResult);
            using var ocrStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonResults));

            await _blobStorageService.UploadDocumentAsync(
                ocrStream,
                ocrBlobName);

            return (true, analyzeResult);
        }
    }
}
