using System;
using System.Threading.Tasks;
using AutoFixture;
using Common.Dto.Response;
using coordinator.Clients.TextExtractor;
using coordinator.Services.TextExtractService;
using Microsoft.Extensions.Logging;
using Moq;
using Polly;
using Polly.Retry;
using Polly.Testing;
using Xunit;

namespace coordinator.tests.Services.TextExtract
{
    public class TextExtractServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<ILogger<TextExtractService>> _mockLogger;
        private readonly Mock<ITextExtractorClient> _mockTextExtractorClient;
        private readonly TextExtractService _textExtractService;
        private readonly ResiliencePipeline<SearchIndexCountResult> _resiliencePipeline;
        private readonly ResiliencePipelineDescriptor _resiliencePipelineDescriptor;
        private const int MaxRetryAttempts = 5;

        public TextExtractServiceTests()
        {
            _fixture = new Fixture();
            _mockLogger = new Mock<ILogger<TextExtractService>>();
            _mockTextExtractorClient = new Mock<ITextExtractorClient>();
            _textExtractService = new TextExtractService(_mockLogger.Object, _mockTextExtractorClient.Object);

            _resiliencePipeline = _textExtractService.GetIndexCountResiliencePipeline(_fixture.Create<Guid>());
            _resiliencePipelineDescriptor = _resiliencePipeline.GetPipelineDescriptor();
        }

        [Fact]
        public void ResiliencePipeline_ProvidesASingleStrategy()
        {
            Assert.Single(_resiliencePipelineDescriptor.Strategies);
        }

        [Fact]
        public void ResiliencePipeline_RetryStrategyUsesExponentialBackoff()
        {
            var retryOptions = Assert.IsType<RetryStrategyOptions<SearchIndexCountResult>>(_resiliencePipelineDescriptor.Strategies[0].Options);

            Assert.Equal(DelayBackoffType.Exponential, retryOptions.BackoffType);
        }

        [Fact]
        public void ResiliencePipeline_RetryStrategyWillAttempt3Retries()
        {
            var retryOptions = Assert.IsType<RetryStrategyOptions<SearchIndexCountResult>>(_resiliencePipelineDescriptor.Strategies[0].Options);

            Assert.Equal(MaxRetryAttempts, retryOptions.MaxRetryAttempts);
        }

        [Fact]
        public void ResiliencePipeline_RetryStrategyWillDelayFor500Milliseconds()
        {
            var retryOptions = Assert.IsType<RetryStrategyOptions<SearchIndexCountResult>>(_resiliencePipelineDescriptor.Strategies[0].Options);

            Assert.Equal(TimeSpan.FromMilliseconds(500), retryOptions.Delay);
        }

        [Fact]
        public async Task WaitForDocumentStoreResultsAsync_ReturnsTrue_WhenIndexedLinesEqualsTargetCount()
        {
            var correlationId = Guid.NewGuid();

            _mockTextExtractorClient.SetupSequence(x => x.GetDocumentIndexCount(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new SearchIndexCountResult(50)))
                .Returns(Task.FromResult(new SearchIndexCountResult(100)));

            var result = await _textExtractService.WaitForDocumentStoreResultsAsync("CMS001", 1234, "1234", 1, 100, correlationId);

            Assert.Equal(100, result.TargetCount);
            Assert.True(result.IsSuccess);
            _mockTextExtractorClient.Verify(x => x.GetDocumentIndexCount("CMS001", 1234, "1234", 1, correlationId), Times.Exactly(2));
        }

        [Fact]
        public async Task WaitForDocumentStoreResultsAsync_ReturnsFalse_WhenIndexedLinesDoesNotEqualTargetCount()
        {
            var correlationId = Guid.NewGuid();

            _mockTextExtractorClient.Setup(x => x.GetDocumentIndexCount(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new SearchIndexCountResult(50)));

            var result = await _textExtractService.WaitForDocumentStoreResultsAsync("CMS001", 1234, "1234", 1, 100, correlationId);

            Assert.Equal(100, result.TargetCount);
            Assert.False(result.IsSuccess);
            _mockTextExtractorClient.Verify(x => x.GetDocumentIndexCount("CMS001", 1234, "1234", 1, correlationId), Times.Exactly(MaxRetryAttempts + 1));
        }

        [Fact]
        public async Task WaitForCaseEmptyResultsAsync_ReturnsTrue_WhenIndexedLinesEqualsZero()
        {
            var correlationId = Guid.NewGuid();

            _mockTextExtractorClient.SetupSequence(x => x.GetCaseIndexCount(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new SearchIndexCountResult(100)))
                .Returns(Task.FromResult(new SearchIndexCountResult(50)))
                .Returns(Task.FromResult(new SearchIndexCountResult(0)));

            var result = await _textExtractService.WaitForCaseEmptyResultsAsync("CMS001", 1234, correlationId);

            Assert.Equal(0, result.TargetCount);
            Assert.True(result.IsSuccess);
            _mockTextExtractorClient.Verify(x => x.GetCaseIndexCount("CMS001", 1234, correlationId), Times.Exactly(3));
        }

        [Fact]
        public async Task WaitForCaseEmptyResultsAsync_ReturnsFalse_WhenIndexedLinesDoesNotEqualZero()
        {
            var correlationId = Guid.NewGuid();

            _mockTextExtractorClient.Setup(x => x.GetCaseIndexCount(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new SearchIndexCountResult(50)));

            var result = await _textExtractService.WaitForCaseEmptyResultsAsync("CMS001", 1234, correlationId);

            Assert.Equal(0, result.TargetCount);
            Assert.False(result.IsSuccess);
            _mockTextExtractorClient.Verify(x => x.GetCaseIndexCount("CMS001", 1234, correlationId), Times.Exactly(MaxRetryAttempts + 1));
        }
    }
}