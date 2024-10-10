using System.Collections.Generic;
using System.Linq;
using Common.Services.BlobStorageService;
using Common.Wrappers;
using coordinator.Domain;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using coordinator.Services.OcrResultsService;
using coordinator.Services.PiiService;
using coordinator.Services.TextSanitizationService;
using FluentAssertions;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace coordinator.tests.Services.PiiServiceTests
{
    public class PiiServiceTests : TestBase
    {
        private readonly PiiService _piiService;
        private readonly Mock<PiiEntityMapper> _piiEntityMapper;
        private readonly OcrResultsService _ocrResultsService;
        private readonly Mock<IPolarisBlobStorageService> _blobStorageService;
        private readonly Mock<IJsonConvertWrapper> _jsonConvertWrapper;
        private readonly Mock<IPiiAllowedListService> _piiAllowedList;
        private readonly Mock<ITextSanitizationService> _textSanitizationService;
        private readonly IConfiguration _configuration;
        private readonly string[] _piiCategories;
        private const int CaseId = 123456;
        private const string DocumentId = "CMS-1000";

        public PiiServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"PiiCategories", "Address;PersonType"},
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _piiEntityMapper = new Mock<PiiEntityMapper>();
            _blobStorageService = new Mock<IPolarisBlobStorageService>();
            _jsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _piiAllowedList = new Mock<IPiiAllowedListService>();
            _piiCategories = new string[] { "Person", "Address", "Email" };
            _textSanitizationService = new Mock<ITextSanitizationService>();
            _piiService = new PiiService(_piiEntityMapper.Object, _blobStorageService.Object, _jsonConvertWrapper.Object, _configuration, _piiAllowedList.Object, _textSanitizationService.Object);

            _ocrResultsService = new OcrResultsService(_blobStorageService.Object, _jsonConvertWrapper.Object);
        }

        [Fact]
        public void WhenCreatingPiiRequests_AMaximumOf5Documents_AreAssignedToEachRequest()
        {
            var documentCharacterLimit = 10;
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

            result.Count().Should().Be(1);
        }

        [Fact]
        public void WhenMappingReconciledPiiToResponse_APiiLineIsCreated_AndWordsPopulated()
        {
            var reconciledPiiEntity = Mother.ReconciledPiiEntity1();
            var expectedLineText = reconciledPiiEntity.LineText;
            var expectedWordCount = reconciledPiiEntity.LineText.Split(' ').Length;
            var reconciledPiiEntities = new List<ReconciledPiiEntity> { reconciledPiiEntity };

            var result = _piiService.MapReconciledPiiToResponse(reconciledPiiEntities);

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

            var result = _piiService.MapReconciledPiiToResponse(reconciledPiiEntities);

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