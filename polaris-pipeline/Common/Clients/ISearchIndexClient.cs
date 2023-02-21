using Common.Domain.SearchIndex;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Clients
{
	public interface ISearchIndexClient
	{
		Task<IList<StreamlinedSearchLine>> Query(int caseId, string searchTerm, Guid correlationId);

        IList<StreamlinedSearchLine> BuildStreamlinedResults(IList<SearchLine> searchResults, string searchTerm, Guid correlationId);
    }
}

