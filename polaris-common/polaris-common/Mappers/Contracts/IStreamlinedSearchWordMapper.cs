using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using polaris_common.Domain.SearchIndex;

namespace polaris_common.Mappers.Contracts
{
    public interface IStreamlinedSearchWordMapper
    {
        StreamlinedWord Map(Word word, string searchTerm, Guid correlationId);
    }
}
