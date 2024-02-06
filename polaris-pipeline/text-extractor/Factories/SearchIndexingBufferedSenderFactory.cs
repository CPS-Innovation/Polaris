using Azure.Search.Documents;
using Common.Domain.SearchIndex;
using text_extractor.Factories.Contracts;

namespace text_extractor.Factories;

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

