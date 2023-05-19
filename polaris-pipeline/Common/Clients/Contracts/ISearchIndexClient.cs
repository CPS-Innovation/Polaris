using Common.Domain.SearchIndex;
using Common.Dto.Tracker;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Clients.Contracts
{
    public interface ISearchIndexClient
    {
        Task<IList<StreamlinedSearchLine>> Query(int caseId, List<BaseTrackerDocumentDto> documents, string searchTerm, Guid correlationId);

        IList<StreamlinedSearchLine> BuildStreamlinedResults(IList<SearchLine> searchResults, string searchTerm, Guid correlationId);
    }
}

