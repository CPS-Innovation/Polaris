using Azure.Search.Documents;
using Common.Domain.SearchIndex;

namespace Common.Factories.Contracts
{
	public interface ISearchIndexingBufferedSenderFactory
	{
		SearchIndexingBufferedSender<SearchLine> Create(SearchClient searchClient);
	}
}

