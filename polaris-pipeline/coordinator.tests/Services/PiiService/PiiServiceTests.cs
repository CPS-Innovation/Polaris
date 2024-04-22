using System.Collections.Generic;
using System.Linq;
using coordinator.Functions.DurableEntity.Entity.Mapper;
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
        private readonly Mock<PiiEntityMapper> _piiEntityMapper;
        private readonly OcrResultsService _ocrResultsService;
        private readonly string[] _piiCategories;
        private const int CaseId = 123456;
        private const string DocumentId = "CMS-1000";

        public PiiServiceTests()
        {
            _configuration = new Mock<IConfiguration>();
            _piiEntityMapper = new Mock<PiiEntityMapper>();
            _piiService = new PiiService(_configuration.Object, _piiEntityMapper.Object);
            _piiCategories = new string[] { "Person", "Address", "Email" };

            _ocrResultsService = new OcrResultsService();
        }

        [Fact]
        public void WhenCreatingPiiRequests_AMaximumOf5Documents_AreAssignedToEachRequest()
        {
            var documentCharacterLimit = 15;
            var processedCount = 0;

            var readResult = new ReadResult
            {
                Page = 1,
                Lines = Mother.OcrLines().ToList()
            };
            var analyzeResults = new AnalyzeResults
            {
                ReadResults = new List<ReadResult> { readResult }
            };

            var piiChunk = new PiiChunk(1, CaseId, DocumentId, documentCharacterLimit);
            piiChunk.BuildChunk(analyzeResults, ref processedCount);

            var result = _piiService.CreatePiiRequests(new List<PiiChunk>() { piiChunk });
        }
    }
}