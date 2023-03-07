#if DEBUG
using System;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Common.Domain.Exceptions;
using Common.Constants;

namespace Common.Services.OcrService
{
    public class MockOcrService : IOcrService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OcrService> _log;

        public MockOcrService(IConfiguration configuration, ILogger<OcrService> log)
        {
            _configuration = configuration;
            _log = log;
        }

        public async Task<AnalyzeResults> GetOcrResultsAsync(string blobName, Guid correlationId)
        {
            _log.LogMethodEntry(correlationId, $"{nameof(MockOcrService)}.{nameof(GetOcrResultsAsync)}", blobName);

            try
            {
                var resultsValue = _configuration[DebugSettings.MockOcrServiceResults];
                AnalyzeResults results = JsonConvert.DeserializeObject<AnalyzeResults>(resultsValue);

                _log.LogMethodFlow(correlationId, nameof(GetOcrResultsAsync), "Mock OCR process completed successfully");

                return await Task.FromResult(results);
            }
            catch (Exception ex)
            {
                _log.LogMethodError(correlationId, nameof(GetOcrResultsAsync), "A Mock OCR Library exception occurred", ex);
                throw new OcrServiceException(ex.Message);
            }
            finally
            {
                _log.LogMethodExit(correlationId, nameof(GetOcrResultsAsync), string.Empty);
            }
        }
    }
}
#endif