using System;
using System.Collections.Generic;
using Common.Domain.Ocr;
using Common.Services.PiiService.Domain.Chunking;

namespace Common.Services.PiiService.Chunking
{
    public interface IPiiChunkingService
    {
        List<PiiChunk> GetDocumentTextPiiChunks(AnalyzeResults analyzeResults, int caseId, string documentId, int characterLimit, Guid correlationId);
    }
}