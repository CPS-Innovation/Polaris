using System;
using RumpoleGateway.Domain.RumpolePipeline;

namespace RumpoleGateway.Factories
{
    public interface IStreamlinedSearchResultFactory
    {
        StreamlinedSearchLine Create(SearchLine searchLine, string searchTerm, Guid correlationId);
    }
}
