using System.Threading.Tasks;

namespace TextExtractor.TestHarness.Services
{
    public interface ITestOcrService
    {
        public Task GetOcrResultsAsync(string filename);
    }
}