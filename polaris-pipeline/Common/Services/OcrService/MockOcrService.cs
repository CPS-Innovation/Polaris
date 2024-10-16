#if DEBUG
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using Common.Services.OcrService.Domain;

namespace Common.Services.OcrService
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

        public Task<OcrOperationResult> GetOperationResultsAsync(Guid operationId, Guid correlationId)
        {
            throw new NotImplementedException();
        }
    }
}
#endif