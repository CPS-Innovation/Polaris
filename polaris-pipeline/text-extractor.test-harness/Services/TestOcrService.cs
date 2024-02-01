using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using text_extractor.Services.OcrService;
using TextExtractor.TestHarness.Extensions;

namespace TextExtractor.TestHarness.Services
{
    public class TestOcrService : ITestOcrService
    {
        private readonly IOcrService _ocrService;

        public TestOcrService(IOcrService ocrService)
        {
            _ocrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
        }

        public async Task GetOcrResultsAsync(string filename)
        {
            var filePath = filename.GetFilePath();

            try
            {
                AnalyzeResults results;

                using (var documentStream = File.Open(filePath, FileMode.Open))
                {
                    results = await _ocrService.GetOcrResultsAsync(documentStream, Guid.NewGuid());
                }

                Console.WriteLine($"OCR result line count: {results.ReadResults.Sum(x => x.Lines.Count())}");
                if (results.ReadResults.Count > 0)
                {
                    Console.WriteLine(results.ReadResults[0].Lines[0].Text);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}