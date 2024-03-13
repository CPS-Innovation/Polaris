using System.Collections.Generic;
using System.Net.Http;
using Common.Domain.SearchIndex;

namespace coordinator.Clients.TextExtractor
{
    public interface ISearchDtoContentFactory
    {
        public StringContent Create(string searchTerm, IEnumerable<SearchFilterDocument> documents);
    }
}