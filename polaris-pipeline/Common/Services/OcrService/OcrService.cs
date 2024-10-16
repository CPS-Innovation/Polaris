using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using Common.Factories.ComputerVisionClientFactory;
using Common.Logging;
using Common.Streaming;
using PolarisDomain = Common.Services.OcrService.Domain;
using Mapster;
using System.Threading;
using Common.Services.OcrService.Domain;

namespace Common.Services.OcrService
{
    public class OcrService : IOcrService
    {
        // As per https://learn.microsoft.com/en-us/azure/ai-services/computer-vision/how-to/call-read-api#get-results-from-the-service
        //  the polling delay is best set between 1 and 2 seconds.  Up until this point we have used 500ms and we are currently experiencing 
        //  the occasional very slow document processing operation.  The article makes it clear that there is a limit in the number of requests
        //  per second, so by increasing the delay we are seeing if we can improve throughput by reducing the number of requests.

        private const int _httpTimeoutMs = 50000; // temporarily set this high so we don't trigger timeouts
        private readonly ComputerVisionClient _computerVisionClient;
        private readonly ILogger<OcrService> _log;

        public OcrService(IComputerVisionClientFactory computerVisionClientFactory,
            ILogger<OcrService> log)
        {
            _computerVisionClient = computerVisionClientFactory.Create();
            _log = log;
        }

        public async Task<Guid> InitiateOperationAsync(Stream stream, Guid correlationId)
        {
            try
            {
                _log.LogMethodFlow(correlationId, nameof(InitiateOperationAsync), $"OCR started");

                // The Computer Vision SDK requires a seekable stream as it will internally retry upon failures (rate limiting, etc.)
                //  and so will need to go through the stream again. Depending on the version/type of framework that is handing us this stream
                //  it may not be seekable.  We have a helper method to ensure it is seekable.
                //  n.b. this incurs an overhead for all executions, the vast majority of which do not need to retry.
                stream = await stream.EnsureSeekableAsync();

                // There have been examples seen in production of ReadInStreamAsync taking 100 seconds and then hitting the default timeout of its internal
                //  HttpClient (presumably).  This would time out our orchestrator execution, so lets time out and throw earlier if we detect a "hanging" call 
                //  and let the caller deal with retries.
                using var cancellationSource = new CancellationTokenSource(_httpTimeoutMs);
                var textHeaders = await _computerVisionClient.ReadInStreamAsync(stream, null, null, "latest", "basic", cancellationSource.Token);

                var operationLocation = textHeaders.OperationLocation;
                const int numberOfCharsInOperationId = 36;
                var operationId = operationLocation[^numberOfCharsInOperationId..];

                _log.LogMethodFlow(correlationId, nameof(InitiateOperationAsync), $"OCR initiated, operation id: {operationId}");

                return Guid.Parse(operationId);
            }
            catch (Exception ex)
            {
                _log.LogMethodError(correlationId, nameof(InitiateOperationAsync), "An OCR Library exception occurred", ex);
                throw;
            }
        }

        public async Task<OcrOperationResult> GetOperationResultsAsync(Guid operationId, Guid correlationId)
        {
            try
            {
                // See cancellationSource comment in InitiateOperationAsync.
                using var cancellationSource = new CancellationTokenSource(_httpTimeoutMs);
                var results = await _computerVisionClient.GetReadResultAsync(operationId, cancellationSource.Token);
                _log.LogMethodFlow(correlationId, nameof(GetOperationResultsAsync), $"OCR read, last updated: {results.LastUpdatedDateTime}, status: {results.Status}, operation id: {operationId}");

                if (results.Status == OperationStatusCodes.Running || results.Status == OperationStatusCodes.NotStarted)
                {
                    return new OcrOperationResult
                    {
                        IsSuccess = false,
                    };
                }

                var elapsedMs = (DateTime.Parse(results.LastUpdatedDateTime) - DateTime.Parse(results.CreatedDateTime)).TotalMilliseconds;
                _log.LogMethodFlow(correlationId, nameof(GetOperationResultsAsync), $"OCR completed in {elapsedMs}ms, status: {results.Status}, pages: {results.AnalyzeResult?.ReadResults?.Count}, operation id: {operationId}");

                if (results.Status == OperationStatusCodes.Failed)
                {
                    throw new Exception($"{nameof(GetOperationResultsAsync)} failed with status {results.Status}, operation id: {operationId}");
                }

                return new OcrOperationResult
                {
                    IsSuccess = true,
                    AnalyzeResults = results.AnalyzeResult.Adapt<PolarisDomain.AnalyzeResults>()
                };
            }
            catch (Exception ex)
            {
                _log.LogMethodError(correlationId, nameof(GetOperationResultsAsync), "An OCR Library exception occurred", ex);
                throw;
            }
        }
    }
}