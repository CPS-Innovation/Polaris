using Azure.Search.Documents;
using polaris_common.Domain.SearchIndex;

namespace polaris_common.Factories.Contracts
{
	public interface ISearchIndexingBufferedSenderFactory
	{
		SearchIndexingBufferedSender<SearchLine> Create(SearchClient searchClient);
	}
}

