using System.Threading.Tasks;
using Azure.AI.TextAnalytics;
using coordinator.Domain;

namespace coordinator.Clients.TextAnalytics
{
    public interface ITextAnalysisClient
    {
        Task<RecognizePiiEntitiesResultCollection> CheckForPii(PiiRequestDto piiRequest);
    }
}