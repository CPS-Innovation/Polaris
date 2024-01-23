using Azure.Search.Documents;
using Common.Domain.SearchIndex;
using Common.Factories.Contracts;

namespace Common.Factories;

public class SearchIndexingBufferedSenderFactory : ISearchIndexingBufferedSenderFactory
{
	public SearchIndexingBufferedSender<ISearchable> Create(SearchClient searchClient)
	{
		return new SearchIndexingBufferedSender<ISearchable>(searchClient,
			new SearchIndexingBufferedSenderOptions<ISearchable>
			{
				KeyFieldAccessor = searchLine => searchLine.Id
			});
	}
}

