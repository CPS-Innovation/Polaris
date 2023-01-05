using System;
using System.Threading.Tasks;
using AutoFixture;
using Common.Domain.DocumentExtraction;
using Common.Services.DocumentExtractionService.Contracts;
using coordinator.Domain;
using coordinator.Functions.ActivityFunctions;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.ActivityFunctions
{
    public class GetCaseDocumentsTests
    {
        private readonly Case _case;
        private readonly GetCaseDocumentsActivityPayload _payload;

        private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;
        
        private readonly GetCaseDocuments _getCaseDocuments;
        
        public GetCaseDocumentsTests()
        {
            var fixture = new Fixture();
            _payload = fixture.Create<GetCaseDocumentsActivityPayload>();
            _case = fixture.Create<Case>();

            var mockDocumentExtractionService = new Mock<IDdeiDocumentExtractionService>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDurableActivityContext.Setup(context => context.GetInput<GetCaseDocumentsActivityPayload>())
                .Returns(_payload);

            mockDocumentExtractionService.Setup(client => client.ListDocumentsAsync(_payload.CaseUrn, _payload.CaseId.ToString(), _payload.UpstreamToken, 
                    _payload.CorrelationId))
                .ReturnsAsync(_case.CaseDocuments);

            var mockLogger = new Mock<ILogger<GetCaseDocuments>>();
            _getCaseDocuments = new GetCaseDocuments(mockDocumentExtractionService.Object, mockLogger.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<GetCaseDocumentsActivityPayload>())
                .Returns(default(GetCaseDocumentsActivityPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => _getCaseDocuments.Run(_mockDurableActivityContext.Object));
        }
        
        [Fact]
        public async Task Run_WhenCaseIdIsZero_ThrowsArgumentException()
        {
            _payload.CaseId = 0;
            _mockDurableActivityContext.Setup(context => context.GetInput<GetCaseDocumentsActivityPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _getCaseDocuments.Run(_mockDurableActivityContext.Object));
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Run_WhenAccessTokenIsNullOrWhitespace_ThrowsArgumentException(string upstreamToken)
        {
            _payload.UpstreamToken = upstreamToken;
            _mockDurableActivityContext.Setup(context => context.GetInput<GetCaseDocumentsActivityPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _getCaseDocuments.Run(_mockDurableActivityContext.Object));
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Run_WhenCaseUrnIsNullOrWhitespace_ThrowsArgumentException(string caseUrn)
        {
            _payload.CaseUrn = caseUrn;
            _mockDurableActivityContext.Setup(context => context.GetInput<GetCaseDocumentsActivityPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _getCaseDocuments.Run(_mockDurableActivityContext.Object));
        }
        
        [Fact]
        public async Task Run_WhenCorrelationIdIsEmpty_ThrowsArgumentException()
        {
            _payload.CorrelationId = Guid.Empty;
            _mockDurableActivityContext.Setup(context => context.GetInput<GetCaseDocumentsActivityPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _getCaseDocuments.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsCaseDocuments()
        {
            var caseDocuments = await _getCaseDocuments.Run(_mockDurableActivityContext.Object);

            caseDocuments.Should().BeEquivalentTo(_case.CaseDocuments);
        }
    }
}
