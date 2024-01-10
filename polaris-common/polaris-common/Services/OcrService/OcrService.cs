using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using polaris_common.Domain.Exceptions;
using polaris_common.Factories.Contracts;
using polaris_common.Logging;
using polaris_common.Services.SasGeneratorService;

namespace polaris_common.Services.OcrService
{
    public class OcrService : IOcrService
    {
        private readonly ComputerVisionClient _computerVisionClient;

        private readonly ILogger<OcrService> _log;

        public OcrService(IComputerVisionClientFactory computerVisionClientFactory,
            ISasGeneratorService sasGeneratorService, ILogger<OcrService> log)
        {
            _computerVisionClient = computerVisionClientFactory.Create();
            _log = log;
        }

        public async Task<AnalyzeResults> GetOcrResultsAsync(Stream stream, Guid correlationId)
        {
            try
            {
                var textHeaders = await _computerVisionClient.ReadInStreamAsync(stream);

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