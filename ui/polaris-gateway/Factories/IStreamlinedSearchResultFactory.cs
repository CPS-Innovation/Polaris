using System;
using PolarisGateway.Domain.PolarisPipeline;

namespace PolarisGateway.Factories
{
    public interface IStreamlinedSearchResultFactory
    {
        StreamlinedSearchLine Create(SearchLine searchLine, string searchTerm, Guid correlationId);
    }
}
