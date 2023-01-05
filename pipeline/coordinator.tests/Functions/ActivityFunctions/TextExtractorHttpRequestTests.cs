using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using coordinator.Domain;
using coordinator.Factories;
using coordinator.Functions.ActivityFunctions;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.ActivityFunctions
{
    public class TextExtractorHttpRequestTests
    {
        private readonly DurableHttpRequest _durableRequest;
        private readonly TextExtractorHttpRequestActivityPayload _payload;

        private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;

        private readonly CreateTextExtractorHttpRequest _createTextExtractorHttpRequest;

        public TextExtractorHttpRequestTests()
        {
            var fixture = new Fixture();
            _payload = fixture.Create<TextExtractorHttpRequestActivityPayload>();
            _durableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.test.co.uk"));

            var mockTextExtractorHttpFactory = new Mock<ITextExtractorHttpRequestFactory>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDurableActivityContext.Setup(context => context.GetInput<TextExtractorHttpRequestActivityPayload>())
                .Returns(_payload);

            mockTextExtractorHttpFactory.Setup(client => client.Create(_payload.CaseId, _payload.DocumentId, _payload.VersionId,
                    _payload.BlobName, _payload.CorrelationId))
                .ReturnsAsync(_durableRequest);

            var mockLogger = new Mock<ILogger<CreateTextExtractorHttpRequest>>();
            _createTextExtractorHttpRequest = new CreateTextExtractorHttpRequest(mockTextExtractorHttpFactory.Object, mockLogger.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<TextExtractorHttpRequestActivityPayload>())
                .Returns(default(TextExtractorHttpRequestActivityPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => _createTextExtractorHttpRequest.Run(_mockDurableActivityContext.Object));
        }
        
        [Fact]
        public async Task Run_WhenCaseIdIsZero_ThrowsArgumentException()
        {
            _payload.CaseId = 0;
            _mockDurableActivityContext.Setup(context => context.GetInput<TextExtractorHttpRequestActivityPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _createTextExtractorHttpRequest.Run(_mockDurableActivityContext.Object));
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Run_WhenDocumentIdIsNullOrWhitespace_ThrowsArgumentException(string documentId)
        {
            _payload.DocumentId = documentId;
            _mockDurableActivityContext.Setup(context => context.GetInput<TextExtractorHttpRequestActivityPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _createTextExtractorHttpRequest.Run(_mockDurableActivityContext.Object));
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Run_WhenBlobNameIsNullOrWhitespace_ThrowsArgumentException(string blobName)
        {
            _payload.BlobName = blobName;
            _mockDurableActivityContext.Setup(context => context.GetInput<TextExtractorHttpRequestActivityPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _createTextExtractorHttpRequest.Run(_mockDurableActivityContext.Object));
        }
        
        [Fact]
        public async Task Run_WhenCorrelationIdIsEmpty_ThrowsArgumentException()
        {
            _payload.CorrelationId = Guid.Empty;
            _mockDurableActivityContext.Setup(context => context.GetInput<TextExtractorHttpRequestActivityPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _createTextExtractorHttpRequest.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsDurableRequest()
        {
            var durableRequest = await _createTextExtractorHttpRequest.Run(_mockDurableActivityContext.Object);

            durableRequest.Should().Be(_durableRequest);
        }
    }
}
