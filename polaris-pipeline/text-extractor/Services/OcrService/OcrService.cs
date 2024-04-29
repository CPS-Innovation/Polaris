using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using text_extractor.Factories.Contracts;

namespace text_extractor.Services.OcrService
{
    public class OcrService : IOcrService
    {
        // As per https://learn.microsoft.com/en-us/azure/ai-services/computer-vision/how-to/call-read-api#get-results-from-the-service
        //  the polling delay is best set between 1 and 2 seconds.  Up until this point we have used 500ms and we are currently experiencing 
        //  the occasional very slow document processing operation.  The article makes it clear that there is a limit in the number of requests
        //  per second, so by increasing the delay we are seeing if we can improve throughput by reducing the number of requests.
        private const int _pollingDelayMs = 2000;
        private readonly ComputerVisionClient _computerVisionClient;
        private readonly ILogger<OcrService> _log;

        public OcrService(IComputerVisionClientFactory computerVisionClientFactory,
            ILogger<OcrService> log)
        {
            _computerVisionClient = computerVisionClientFactory.Create();
            _log = log;
        }

        public async Task<AnalyzeResults> GetOcrResultsAsync(Stream stream, Guid correlationId)
        {
            try
            {
                var watch = new Stopwatch();
                watch.Start();

                var textHeaders = await _computerVisionClient.ReadInStreamAsync(stream);
                var operationLocation = textHeaders.OperationLocation;

                const int numberOfCharsInOperationId = 36;
                var operationId = operationLocation[^numberOfCharsInOperationId..];

                ReadOperationResult results;
                while (true)
                {
                    // always wait before the first read attempt, it will not be ready immediately
                    await Task.Delay(_pollingDelayMs);

                    results = await _computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));

                    if (results.Status is OperationStatusCodes.Failed or OperationStatusCodes.Succeeded)
                    {
                        break;
                    }
                }

                watch.Stop();
                _log.LogMethodFlow(correlationId, nameof(GetOcrResultsAsync), $"OCR completed in {watch.ElapsedMilliseconds}ms");

                return results.AnalyzeResult;
            }
            catch (Exception ex)
            {
                _log.LogMethodError(correlationId, nameof(GetOcrResultsAsync), "An OCR Library exception occurred", ex);
                throw new OcrServiceException(ex.Message);
            }
        }
    }
}