using System;
using System.Collections.Generic;
using System.Net.Http;
using Common.Domain.SearchIndex;

namespace Common.Factories.Contracts
{
    public interface IPipelineClientSearchRequestFactory
    {
        HttpRequestMessage Create(long cmsCaseId, string searchTerm, Guid correlationId, IEnumerable<SearchFilterDocument> documents);
    }
}