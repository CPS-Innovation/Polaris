using System.Threading.Tasks;

namespace TextExtractor.TestHarness.Services
{
    public interface ITextExtractorService
    {
        public Task ExtractAndStoreDocument(string filename);
    }
}