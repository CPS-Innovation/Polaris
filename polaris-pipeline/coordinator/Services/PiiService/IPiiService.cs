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
        Task ReconcilePiiResults(IList<PiiChunk> piiChunks, RecognizePiiEntitiesResultCollection piiResults);
    }
}