#if DEBUG
using System;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;

namespace text_extractor.Services.OcrService
{
    public class MockOcrService : IOcrService
    {
        public static string MockOcrServiceSetting = "POLARIS_MOCK_OCR_SERVICE";
        private const string MockOcrServiceResults = nameof(MockOcrServiceResults);
        private readonly IConfiguration _configuration;
        private readonly ILogger<OcrService> _log;

        public MockOcrService(IConfiguration configuration, ILogger<OcrService> log)
        {
            _configuration = configuration;
            _log = log;
        }

        public async Task<AnalyzeResults> GetOcrResultsAsync(Stream stream, Guid correlationId)
        {
            _log.LogMethodEntry(correlationId, $"{nameof(MockOcrService)}.{nameof(GetOcrResultsAsync)}", nameof(stream));

            try
            {
                var resultsValue = _configuration[MockOcrServiceResults];
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