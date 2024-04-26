using System.Collections.Generic;
using System.Linq;
using coordinator.Domain;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using coordinator.Services.OcrResultsService;
using coordinator.Services.PiiService;
using FluentAssertions;
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

        [Fact]
        public void WhenMappingReconciledPiiToResponse_APiiLineIsCreated_AndWordsPopulated()
        {
            var reconciledPiiEntity = Mother.ReconciledPiiEntity1();
            var expectedLineText = reconciledPiiEntity.LineText;
            var expectedWordCount = reconciledPiiEntity.LineText.Split(' ').Length;
            var reconciledPiiEntities = new List<ReconciledPiiEntity> { reconciledPiiEntity };

            var result = PiiService.MapReconcilledPiiToResponse(reconciledPiiEntities);

            result.Count().Should().Be(1);
            result.First().Text.Should().Be(expectedLineText);
            result.First().Words.Count.Should().Be(expectedWordCount);
        }

        [Fact]
        public void WhenMappingReconciledPiiToResponse_TheWordsIdentifiedAsPii_HaveBoundingBoxValues()
        {
            var reconciledPiiEntity1 = Mother.ReconciledPiiEntity1();
            var reconciledPiiEntity2 = Mother.ReconciledPiiEntity2();
            var reconciledPiiEntity3 = Mother.ReconciledPiiEntity3();
            var reconciledPiiEntities = new List<ReconciledPiiEntity> {
                reconciledPiiEntity1,
                reconciledPiiEntity2,
                reconciledPiiEntity3
            };

            var result = PiiService.MapReconcilledPiiToResponse(reconciledPiiEntities);

            result.Count().Should().Be(1);
            result.First().Words.Count(x => x.BoundingBox != null).Should().Be(reconciledPiiEntities.Count);
            result.First().Words[0].BoundingBox.Should().BeNull();
            result.First().Words[1].BoundingBox.Should().BeNull();
            result.First().Words[2].BoundingBox.Should().BeNull();
            result.First().Words[3].BoundingBox.Should().NotBeNullOrEmpty();
            result.First().Words[4].BoundingBox.Should().BeNull();
            result.First().Words[5].BoundingBox.Should().BeNull();
            result.First().Words[6].BoundingBox.Should().BeNull();
            result.First().Words[7].BoundingBox.Should().BeNull();
            result.First().Words[8].BoundingBox.Should().BeNull();
            result.First().Words[9].BoundingBox.Should().BeNull();
            result.First().Words[10].BoundingBox.Should().BeNull();
            result.First().Words[11].BoundingBox.Should().NotBeNullOrEmpty();
            result.First().Words[12].BoundingBox.Should().NotBeNullOrEmpty();
            result.First().Words[13].BoundingBox.Should().BeNull();
            result.First().Words[14].BoundingBox.Should().BeNull();
        }
    }
}