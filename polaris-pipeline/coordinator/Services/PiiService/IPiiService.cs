using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.AI.TextAnalytics;
using Common.Domain.Pii;
using coordinator.Domain;
using coordinator.Services.OcrResultsService;

namespace coordinator.Services.PiiService
{
    public interface IPiiService
    {
        IEnumerable<PiiRequestDto> CreatePiiRequests(List<PiiChunk> piiChunks);
        IEnumerable<PiiLine> ReconcilePiiResults(IList<PiiChunk> piiChunks, PiiEntitiesWrapper piiResults);
        PiiEntitiesWrapper MapPiiResults(RecognizePiiEntitiesResultCollection[] piiResults);
        Task<PiiEntitiesWrapper> GetPiiResultsFromBlob(int caseId, string documentId, Guid correlationId);
    }
}