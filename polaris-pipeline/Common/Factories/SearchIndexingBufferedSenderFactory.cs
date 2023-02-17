using Azure.Search.Documents;
using Common.Domain.SearchIndex;
using Common.Factories.Contracts;

namespace Common.Factories;

public class SearchIndexingBufferedSenderFactory: ISearchIndexingBufferedSenderFactory
{
	public SearchIndexingBufferedSender<SearchLine> Create(SearchClient searchClient)
    {
		return new SearchIndexingBufferedSender<SearchLine>(searchClient,
			new SearchIndexingBufferedSenderOptions<SearchLine>
				{
					KeyFieldAccessor = searchLine => searchLine.Id
				});
	}
}

