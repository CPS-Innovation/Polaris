using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.AI.TextAnalytics;
using coordinator.Domain;
using coordinator.Services.OcrResultsService;

namespace coordinator.Services.PiiService
{
    public interface IPiiService
    {
        IEnumerable<PiiRequestDto> CreatePiiRequests(List<PiiChunk> piiChunks);
        List<ReconciledPiiEntity> ReconcilePiiResults(IList<PiiChunk> piiChunks, PiiEntitiesWrapper piiResults);
        PiiEntitiesWrapper MapPiiResults(RecognizePiiEntitiesResultCollection[] piiResults);
    }
}