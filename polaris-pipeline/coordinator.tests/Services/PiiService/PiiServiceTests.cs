using System.Collections.Generic;
using System.Linq;
using coordinator.Services.OcrResultsService;
using coordinator.Services.PiiService;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace coordinator.tests.Services.PiiServiceTests
{
    public class PiiServiceTests
    {
        private readonly Mock<IConfiguration> _configuration;
        private readonly PiiService _piiService;
        private readonly OcrResultsService _ocrResultsService;
        private readonly string[] _piiCategories;
        private const int CaseId = 123456;
        private const string DocumentId = "CMS-1000";

        public PiiServiceTests()
        {
            _configuration = new Mock<IConfiguration>();
            _piiService = new PiiService(_configuration.Object);
            _piiCategories = new string[] { "Person", "Address", "Email" };

            _ocrResultsService = new OcrResultsService();
        }

        [Fact]
        public void WhenCreatingPiiRequests_AMaximumOf5Documents_AreAssignedToEachRequest()
        {
            var documentCharacterLimit = 15;
            var readResult = new ReadResult
            {
                Page = 1,
                Lines = Mother.OcrLines().ToList()
            };
            var analyzeResults = new AnalyzeResults
            {
                ReadResults = new List<ReadResult> { readResult }
            };

            var piiChunk = new PiiChunk(1, CaseId, DocumentId, documentCharacterLimit, 0);
            piiChunk.BuildChunk(analyzeResults);

            var result = _piiService.CreatePiiRequests(new List<PiiChunk>() { piiChunk });

        }

        [Fact]
        public void WhenCreatingPiiRequests_ThePiiCategories_AreAssignedToEachRequest()
        {

        }
    }
}