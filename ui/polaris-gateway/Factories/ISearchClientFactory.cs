using Azure.Search.Documents;

namespace PolarisGateway.Factories
{
	public interface ISearchClientFactory
	{
		SearchClient Create();
	}
}

