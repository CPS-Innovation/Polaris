using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace coordinator.Services.OcrResultsService
{
    public interface IOcrResultsService
    {
        OcrResult GetDocumentText(AnalyzeResults analyzeResults);
    }
}