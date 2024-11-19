using System.Net.Http;

namespace coordinator.Clients.TextExtractor
{
    public interface ISearchDtoContentFactory
    {
        public StringContent Create(string searchTerm);
    }
}