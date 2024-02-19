using Azure.Search.Documents;

namespace text_extractor.Factories.Contracts;

public interface IAzureSearchClientFactory
{
    SearchClient Create();
}
