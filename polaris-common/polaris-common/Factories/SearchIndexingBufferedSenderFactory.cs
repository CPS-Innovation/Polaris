using Azure.Search.Documents;
using polaris_common.Domain.SearchIndex;
using polaris_common.Factories.Contracts;

namespace polaris_common.Factories;

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

