using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace polaris_common.Services.OcrService
{
    public interface IOcrService
    {
        Task<AnalyzeResults> GetOcrResultsAsync(Stream stream, Guid correlationId);
    }
}

