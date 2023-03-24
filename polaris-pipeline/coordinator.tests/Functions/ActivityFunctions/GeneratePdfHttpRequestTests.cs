using System;
using System.Net.Http;
using AutoFixture;
using coordinator.Domain;
using coordinator.Factories;
using coordinator.Functions.ActivityFunctions.Document;
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

            mockGeneratePdfHttpFactory.Setup(client => client.Create(_payload.CmsCaseUrn, _payload.CmsCaseId, _payload.DocumentCategory,
                _payload.DocumentId, _payload.FileName, _payload.VersionId, _payload.CmsAuthValues, _payload.CorrelationId)).Returns(_durableRequest);

            var mockLogger = new Mock<ILogger<CreateGeneratePdfHttpRequest>>();
            _createGeneratePdfHttpRequest = new CreateGeneratePdfHttpRequest(mockGeneratePdfHttpFactory.Object, mockLogger.Object);
        }

        [Fact]
        public void Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<GeneratePdfHttpRequestActivityPayload>())
                .Returns(default(GeneratePdfHttpRequestActivityPayload));

            Assert.Throws<ArgumentException>(() => _createGeneratePdfHttpRequest.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public void Run_WhenCaseIdIsZero_ThrowsArgumentException()
        {
            _payload.CmsCaseId = 0;
            _mockDurableActivityContext.Setup(context => context.GetInput<GeneratePdfHttpRequestActivityPayload>())
                .Returns(_payload);

            Assert.Throws<ArgumentException>(() => _createGeneratePdfHttpRequest.Run(_mockDurableActivityContext.Object));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Run_WhenDocumentIdIsNullOrWhitespace_ThrowsArgumentException(string documentId)
        {
            _payload.DocumentId = documentId;
            _mockDurableActivityContext.Setup(context => context.GetInput<GeneratePdfHttpRequestActivityPayload>())
                .Returns(_payload);

            Assert.Throws<ArgumentException>(() => _createGeneratePdfHttpRequest.Run(_mockDurableActivityContext.Object));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Run_WhenFileNameIsNullOrWhitespace_ThrowsArgumentException(string fileName)
        {
            _payload.FileName = fileName;
            _mockDurableActivityContext.Setup(context => context.GetInput<GeneratePdfHttpRequestActivityPayload>())
                .Returns(_payload);

            Assert.Throws<ArgumentException>(() => _createGeneratePdfHttpRequest.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public void Run_WhenCorrelationIdIsEmpty_ThrowsArgumentException()
        {
            _payload.CorrelationId = Guid.Empty;
            _mockDurableActivityContext.Setup(context => context.GetInput<GeneratePdfHttpRequestActivityPayload>())
                .Returns(_payload);

            Assert.Throws<ArgumentException>(() => _createGeneratePdfHttpRequest.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public void Run_ReturnsDurableRequest()
        {
            var durableRequest = _createGeneratePdfHttpRequest.Run(_mockDurableActivityContext.Object);

            durableRequest.Should().Be(_durableRequest);
        }
    }
}
