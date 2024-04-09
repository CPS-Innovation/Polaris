using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace coordinator.Services.OcrResultsService
{
    public interface IOcrResultsService
    {
        List<PiiChunk> GetDocumentText(AnalyzeResults analyzeResults, int characterLimit);
    }
}