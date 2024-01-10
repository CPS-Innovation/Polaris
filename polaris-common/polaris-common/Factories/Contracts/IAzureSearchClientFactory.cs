using Azure.Search.Documents;

namespace polaris_common.Factories.Contracts;

public interface IAzureSearchClientFactory
{
    SearchClient Create();
}
