using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolarisGateway.Domain.PolarisPipeline;

namespace PolarisGateway.Clients.PolarisPipeline
{
	public interface ISearchIndexClient
	{
		Task<IList<StreamlinedSearchLine>> Query(int caseId, string searchTerm, Guid correlationId);

        IList<StreamlinedSearchLine> BuildStreamlinedResults(IList<SearchLine> searchResults, string searchTerm, Guid correlationId);
    }
}

