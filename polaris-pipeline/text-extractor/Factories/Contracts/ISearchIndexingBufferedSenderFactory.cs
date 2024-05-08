using Azure.Search.Documents;
using Common.Domain.SearchIndex;

namespace text_extractor.Factories.Contracts
{
	public interface ISearchIndexingBufferedSenderFactory
	{
		SearchIndexingBufferedSender<ISearchable> Create(SearchClient searchClient);
	}
}

