using System.Threading.Tasks;
using Azure.AI.TextAnalytics;
using Common.Dto.Request;


namespace Common.Services.PiiService.TextAnalytics
{
    public interface ITextAnalysisClient
    {
        Task<RecognizePiiEntitiesResultCollection> CheckForPii(PiiRequestDto piiRequest);
    }
}