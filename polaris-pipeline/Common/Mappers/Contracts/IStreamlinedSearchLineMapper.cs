using Common.Domain.SearchIndex;
using System;

namespace Common.Mappers.Contracts
{
    public interface IStreamlinedSearchLineMapper
    {
        StreamlinedSearchLine Map(SearchLine searchLine, Guid correlationId);
    }
}
