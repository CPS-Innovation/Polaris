using Azure.Search.Documents;

namespace Common.Factories.Contracts;

public interface ISearchClientFactory
{
    SearchClient Create();
}
