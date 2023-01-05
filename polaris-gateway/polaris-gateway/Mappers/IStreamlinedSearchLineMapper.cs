using System;
using PolarisGateway.Domain.PolarisPipeline;

namespace PolarisGateway.Mappers
{
    public interface IStreamlinedSearchLineMapper
    {
        StreamlinedSearchLine Map(SearchLine searchLine, Guid correlationId);
    }
}
