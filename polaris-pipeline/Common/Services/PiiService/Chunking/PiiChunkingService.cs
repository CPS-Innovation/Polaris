using System;
using System.Collections.Generic;
using System.Linq;
using Common.Domain.Ocr;
using Common.Services.PiiService.Domain.Chunking;

namespace Common.Services.PiiService.Chunking
{
    public class PiiChunkingService : IPiiChunkingService
    {
        public List<PiiChunk> GetDocumentTextPiiChunks(AnalyzeResults analyzeResults, int caseId, string documentId, int characterLimit, Guid correlationId) // char limit should be a config value
        {
            var chunks = new List<PiiChunk>();
            var chunkId = 1;
            var currentCharacterLimit = characterLimit;
            var linesToProcessCount = analyzeResults.ReadResults.Sum(x => x.Lines.Count);
            var processedCount = 0;

            while (processedCount < linesToProcessCount)
            {
                var piiChunk = new PiiChunk(chunkId, caseId, documentId, currentCharacterLimit);
                piiChunk.BuildChunk(analyzeResults, ref processedCount);
                chunks.Add(piiChunk);
                chunkId++;
            }

            return chunks;
        }
    }
}