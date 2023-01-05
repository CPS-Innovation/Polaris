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
    public class GeneratePdfHttpRequestTests
    {
        private readonly GeneratePdfHttpRequestActivityPayload _payload;
        private readonly DurableHttpRequest _durableRequest;

        private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;

        private readonly CreateGeneratePdfHttpRequest _createGeneratePdfHttpRequest;

        public GeneratePdfHttpRequestTests()
        {
            var fixture = new Fixture();
            _payload = fixture.Create<GeneratePdfHttpRequestActivityPayload>();
            _durableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.test.co.uk"));

            var mockGeneratePdfHttpFactory = new Mock<IGeneratePdfHttpRequestFactory>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDurableActivityContext.Setup(context => context.GetInput<GeneratePdfHttpRequestActivityPayload>())
                .Returns(_payload);

            mockGeneratePdfHttpFactory.Setup(client => client.Create(_payload.CaseUrn, _payload.CaseId, _payload.DocumentCategory, 
                _payload.DocumentId, _payload.FileName, _payload.VersionId, _payload.UpstreamToken, _payload.CorrelationId)).ReturnsAsync(_durableRequest);

            var mockLogger = new Mock<ILogger<CreateGeneratePdfHttpRequest>>();
            _createGeneratePdfHttpRequest = new CreateGeneratePdfHttpRequest(mockGeneratePdfHttpFactory.Object, mockLogger.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<GeneratePdfHttpRequestActivityPayload>())
                .Returns(default(GeneratePdfHttpRequestActivityPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => _createGeneratePdfHttpRequest.Run(_mockDurableActivityContext.Object));
        }
        
        [Fact]
        public async Task Run_WhenCaseIdIsZero_ThrowsArgumentException()
        {
            _payload.CaseId = 0;
            _mockDurableActivityContext.Setup(context => context.GetInput<GeneratePdfHttpRequestActivityPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _createGeneratePdfHttpRequest.Run(_mockDurableActivityContext.Object));
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Run_WhenDocumentIdIsNullOrWhitespace_ThrowsArgumentException(string documentId)
        {
            _payload.DocumentId = documentId;
            _mockDurableActivityContext.Setup(context => context.GetInput<GeneratePdfHttpRequestActivityPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _createGeneratePdfHttpRequest.Run(_mockDurableActivityContext.Object));
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Run_WhenFileNameIsNullOrWhitespace_ThrowsArgumentException(string fileName)
        {
            _payload.FileName = fileName;
            _mockDurableActivityContext.Setup(context => context.GetInput<GeneratePdfHttpRequestActivityPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _createGeneratePdfHttpRequest.Run(_mockDurableActivityContext.Object));
        }
        
        [Fact]
        public async Task Run_WhenCorrelationIdIsEmpty_ThrowsArgumentException()
        {
            _payload.CorrelationId = Guid.Empty;
            _mockDurableActivityContext.Setup(context => context.GetInput<GeneratePdfHttpRequestActivityPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _createGeneratePdfHttpRequest.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsDurableRequest()
        {
            var durableRequest = await _createGeneratePdfHttpRequest.Run(_mockDurableActivityContext.Object);

            durableRequest.Should().Be(_durableRequest);
        }
    }
}
