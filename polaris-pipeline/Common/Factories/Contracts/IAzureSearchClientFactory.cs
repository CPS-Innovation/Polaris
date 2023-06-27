using Azure.Search.Documents;

namespace Common.Factories.Contracts;

public interface IAzureSearchClientFactory
{
    SearchClient Create();
}
