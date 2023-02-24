using System;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using Common.Domain.Exceptions;
using Common.Factories.Contracts;
using Common.Services.SasGeneratorService;

namespace Common.Services.OcrService
{
    public class OcrService : IOcrService
    {
        private readonly ComputerVisionClient _computerVisionClient;
        private readonly ISasGeneratorService _sasGeneratorService;
        private readonly ILogger<OcrService> _log;

        public OcrService(IComputerVisionClientFactory computerVisionClientFactory,
            ISasGeneratorService sasGeneratorService, ILogger<OcrService> log)
        {
            _computerVisionClient = computerVisionClientFactory.Create();
            _sasGeneratorService = sasGeneratorService;
            _log = log;
        }

        public async Task<AnalyzeResults> GetOcrResultsAsync(string blobName, Guid correlationId)
        {
            _log.LogMethodEntry(correlationId, nameof(GetOcrResultsAsync), blobName);
            _log.LogMethodFlow(correlationId, nameof(GetOcrResultsAsync), "Generate Shared Access Signature encrypted uri to pass to the computer vision client to begin the OCR process");
            var sasLink = await _sasGeneratorService.GenerateSasUrlAsync(blobName, correlationId);

            try
            {
                _log.LogMethodFlow(correlationId, nameof(GetOcrResultsAsync), $"Calling the computer vision client ReadAsync method to begin the OCR process, passing in '{sasLink}'");
                var textHeaders = await _computerVisionClient.ReadAsync(sasLink);

                var operationLocation = textHeaders.OperationLocation;
                await Task.Delay(500);

                const int numberOfCharsInOperationId = 36;
                var operationId = operationLocation[^numberOfCharsInOperationId..];

                ReadOperationResult results;

                while (true)
                {
                    results = await _computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));

                    if (results.Status is OperationStatusCodes.Running or OperationStatusCodes.NotStarted)
                    {
                        await Task.Delay(500);
                    }
                    else
                    {
                        break;
                    }
                }

                _log.LogMethodFlow(correlationId, nameof(GetOcrResultsAsync), "OCR process completed successfully");
                return results.AnalyzeResult;
            }
            catch (Exception ex)
            {
                _log.LogMethodError(correlationId, nameof(GetOcrResultsAsync), "An OCR Library exception occurred", ex);
                throw new OcrServiceException(ex.Message);
            }
            finally
            {
                _log.LogMethodExit(correlationId, nameof(GetOcrResultsAsync), string.Empty);
            }
        }
    }
}