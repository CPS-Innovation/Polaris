using System;
using System.Collections.Generic;
using System.Net.Http;
using Common.Domain.SearchIndex;

namespace coordinator.Factories
{
    public interface IPipelineClientSearchRequestFactory
    {
        HttpRequestMessage Create(long cmsCaseId, string searchTerm, Guid correlationId, IEnumerable<SearchFilterDocument> documents);
    }
}