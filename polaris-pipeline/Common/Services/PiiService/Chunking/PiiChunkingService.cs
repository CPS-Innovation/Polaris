using System.Collections.Generic;
using System.Linq;
using Common.Domain.Ocr;
using Common.Services.PiiService.Domain.Chunking;

namespace Common.Services.PiiService.Chunking
{
    public class PiiChunkingService : IPiiChunkingService
    {
        public List<PiiChunk> GetDocumentTextPiiChunks(AnalyzeResults analyzeResults, int characterLimit) // char limit should be a config value
        {
            var chunks = new List<PiiChunk>();
            var chunkId = 1;
            var currentCharacterLimit = characterLimit;
            var linesToProcessCount = analyzeResults.ReadResults.Sum(x => x.Lines.Count);
            var processedCount = 0;

            while (processedCount < linesToProcessCount)
            {
                var piiChunk = new PiiChunk(chunkId, currentCharacterLimit);
                piiChunk.BuildChunk(analyzeResults, ref processedCount);
                chunks.Add(piiChunk);
                chunkId++;
            }

            return chunks;
        }
    }
}