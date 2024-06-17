#if DEBUG
using System;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using coordinator.Services.OcrService.Domain;

namespace coordinator.Services.OcrService
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

        public Task<Guid> InitiateOperationAsync(Stream stream, Guid correlationId)
        {
            throw new NotImplementedException();
        }

        public Task<(bool, AnalyzeResults)> GetOperationResultsAsync(Guid operationId, Guid correlationId)
        {
            throw new NotImplementedException();
        }
    }
}
#endif