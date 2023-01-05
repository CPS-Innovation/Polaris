using Azure.Search.Documents;

namespace RumpoleGateway.Factories
{
	public interface ISearchClientFactory
	{
		SearchClient Create();
	}
}

