using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common.Services.BlobStorageService;
using Common.Wrappers;
using Common.Services.OcrService;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Common.Domain.Ocr;

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
            var ocrOperationResult = await _ocrService.GetOperationResultsAsync(operationId, correlationId);

            if (!ocrOperationResult.IsSuccess)
            {
                return (false, null);
            }

            var jsonResults = _jsonConvertWrapper.SerializeObject(ocrOperationResult.AnalyzeResults);
            using var ocrStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonResults));

            await _blobStorageService.UploadBlobAsync(
                ocrStream,
                ocrBlobName);

            return (true, ocrOperationResult.AnalyzeResults);
        }
    }
}
