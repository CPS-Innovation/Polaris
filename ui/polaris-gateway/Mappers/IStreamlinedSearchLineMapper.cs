using System;
using RumpoleGateway.Domain.RumpolePipeline;

namespace RumpoleGateway.Mappers
{
    public interface IStreamlinedSearchLineMapper
    {
        StreamlinedSearchLine Map(SearchLine searchLine, Guid correlationId);
    }
}
