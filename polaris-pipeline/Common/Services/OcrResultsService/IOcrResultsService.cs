using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Common.Services.OcrResultsService
{
    public interface IOcrResultsService
    {
        Task<AnalyzeResults> GetOcrResultsFromBlob(int caseId, string documentId, Guid correlationId);
        List<PiiChunk> GetDocumentTextPiiChunks(AnalyzeResults analyzeResults, int caseId, string documentId, int characterLimit, Guid correlationId);
    }
}