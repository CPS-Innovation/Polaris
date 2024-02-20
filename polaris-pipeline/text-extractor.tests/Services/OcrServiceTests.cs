using System;
using System.Net.Http;
using AutoFixture;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Moq;
using Polly;
using Polly.Retry;
using Polly.Testing;
using text_extractor.Factories.Contracts;
using text_extractor.Services.OcrService;
using Xunit;

namespace text_extractor.tests.Services
{
    public class OcrServiceTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IComputerVisionClientFactory> _mockComputerVisionClientFactory;
        private readonly Mock<IComputerVisionClient> _mockComputerVisionClient;
        private readonly Mock<ILogger<OcrService>> _mockLogger;
        private readonly OcrService _ocrService;
        private readonly ResiliencePipeline<HttpOperationHeaderResponse<ReadInStreamHeaders>> _streamPipeline;
        private readonly ResiliencePipeline<HttpOperationResponse<ReadOperationResult>> _readPipeline;

        public OcrServiceTests()
        {
            _fixture = new Fixture();

            _mockComputerVisionClient = new Mock<IComputerVisionClient>();

            _mockComputerVisionClientFactory = new Mock<IComputerVisionClientFactory>();
            _mockComputerVisionClientFactory.Setup(x => x.Create()).Returns(new ComputerVisionClient(_fixture.Create<ApiKeyServiceClientCredentials>(), It.IsAny<DelegatingHandler[]>()));

            _mockLogger = new Mock<ILogger<OcrService>>();

            _ocrService = new OcrService(_mockComputerVisionClientFactory.Object, _mockLogger.Object);
            _streamPipeline = _ocrService.GetReadInStreamComputerVisionResiliencePipeline(Guid.NewGuid());
            _readPipeline = _ocrService.GetReadResultsComputerVisionResiliencePipeline(Guid.NewGuid());
        }

        [Fact]
        public void StreamResiliencePipeline_ProvidesASingleStrategy()
        {
            var descriptor = _streamPipeline.GetPipelineDescriptor();

            Assert.Single(descriptor.Strategies);
        }

        [Fact]
        public void StreamResiliencePipeline_RetryStrategyUsesExponentialBackoff()
        {
            var descriptor = _streamPipeline.GetPipelineDescriptor();
            var retryOptions = Assert.IsType<RetryStrategyOptions<HttpOperationHeaderResponse<ReadInStreamHeaders>>>(descriptor.Strategies[0].Options);

            Assert.Equal(DelayBackoffType.Exponential, retryOptions.BackoffType);
        }

        [Fact]
        public void StreamResiliencePipeline_RetryStrategyWillAttempt3Retries()
        {
            var descriptor = _streamPipeline.GetPipelineDescriptor();
            var retryOptions = Assert.IsType<RetryStrategyOptions<HttpOperationHeaderResponse<ReadInStreamHeaders>>>(descriptor.Strategies[0].Options);

            Assert.Equal(3, retryOptions.MaxRetryAttempts);
        }

        [Fact]
        public void StreamResiliencePipeline_RetryStrategyWillDelayFor5000Milliseconds()
        {
            var descriptor = _streamPipeline.GetPipelineDescriptor();
            var retryOptions = Assert.IsType<RetryStrategyOptions<HttpOperationHeaderResponse<ReadInStreamHeaders>>>(descriptor.Strategies[0].Options);

            Assert.Equal(TimeSpan.FromMilliseconds(5000), retryOptions.Delay);
        }

        [Fact]
        public void ReadResiliencePipeline_ProvidesASingleStrategy()
        {
            var descriptor = _readPipeline.GetPipelineDescriptor();

            Assert.Single(descriptor.Strategies);
        }

        [Fact]
        public void ReadResiliencePipeline_RetryStrategyUsesExponentialBackoff()
        {
            var descriptor = _readPipeline.GetPipelineDescriptor();
            var retryOptions = Assert.IsType<RetryStrategyOptions<HttpOperationResponse<ReadOperationResult>>>(descriptor.Strategies[0].Options);

            Assert.Equal(DelayBackoffType.Exponential, retryOptions.BackoffType);
        }

        [Fact]
        public void ReadResiliencePipeline_RetryStrategyWillAttempt3Retries()
        {
            var descriptor = _readPipeline.GetPipelineDescriptor();
            var retryOptions = Assert.IsType<RetryStrategyOptions<HttpOperationResponse<ReadOperationResult>>>(descriptor.Strategies[0].Options);

            Assert.Equal(3, retryOptions.MaxRetryAttempts);
        }

        [Fact]
        public void ReadResiliencePipeline_RetryStrategyWillDelayFor5000Milliseconds()
        {
            var descriptor = _readPipeline.GetPipelineDescriptor();
            var retryOptions = Assert.IsType<RetryStrategyOptions<HttpOperationResponse<ReadOperationResult>>>(descriptor.Strategies[0].Options);

            Assert.Equal(TimeSpan.FromMilliseconds(5000), retryOptions.Delay);
        }
    }
}