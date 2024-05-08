using Common.Domain.SearchIndex;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace text_extractor.Mappers.Contracts
{
    public interface IStreamlinedSearchWordMapper
    {
        StreamlinedWord Map(Word word, string searchTerm);
    }
}
