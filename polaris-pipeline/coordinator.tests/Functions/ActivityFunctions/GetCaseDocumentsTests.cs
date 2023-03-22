using System.Security.AccessControl;
using System;
using System.Threading.Tasks;
using AutoFixture;
using Common.Domain.DocumentExtraction;
using Common.Services.DocumentExtractionService.Contracts;
using coordinator.Domain;
using coordinator.Domain.Tracker;
using coordinator.Functions.ActivityFunctions;
using coordinator.Mappers;
using coordinator.Services.DocumentToggle;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using coordinator.Domain.Tracker.Presentation;

namespace coordinator.tests.Functions.ActivityFunctions
{
    public class GetCaseDocumentsTests
    {
        private readonly CmsCaseDocument[] _caseDocuments;

        private readonly TransitionDocument[] _transitionDocuments;

        private readonly PresentationFlags[] _presentationFlags;

        private readonly GetCaseDocumentsActivityPayload _payload;

        private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;

        private readonly GetCaseDocuments _getCaseDocuments;

        public GetCaseDocumentsTests()
        {
            var fixture = new Fixture();
            _payload = fixture.Create<GetCaseDocumentsActivityPayload>();
            _caseDocuments = new[] {
              fixture.Create<CmsCaseDocument>(),
              fixture.Create<CmsCaseDocument>()
            };

            _transitionDocuments = new[] {
              fixture.Create<TransitionDocument>(),
              fixture.Create<TransitionDocument>()
            };

            _presentationFlags = new[] {
              fixture.Create<PresentationFlags>(),
              fixture.Create<PresentationFlags>()
            };

            var mockDocumentExtractionService = new Mock<IDdeiDocumentExtractionService>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDurableActivityContext.Setup(context => context.GetInput<GetCaseDocumentsActivityPayload>())
                .Returns(_payload);

            mockDocumentExtractionService.Setup(client => client.ListDocumentsAsync(_payload.CmsCaseUrn, _payload.CmsCaseId.ToString(),
                    _payload.CmsAuthValues, _payload.CorrelationId))
                .ReturnsAsync(_caseDocuments);

            var mockTransitionDocumentMapper = new Mock<ITransitionDocumentMapper>();
            mockTransitionDocumentMapper
              .Setup(mapper => mapper.MapToTransitionDocument(_caseDocuments[0]))
              .Returns(_transitionDocuments[0]);
            mockTransitionDocumentMapper
              .Setup(mapper => mapper.MapToTransitionDocument(_caseDocuments[1]))
              .Returns(_transitionDocuments[1]);


            var mockDocumentToggleService = new Mock<IDocumentToggleService>();
            mockDocumentToggleService
              .Setup(service => service.GetDocumentPresentationFlags(_transitionDocuments[0]))
              .Returns(_presentationFlags[0]);
            mockDocumentToggleService
              .Setup(service => service.GetDocumentPresentationFlags(_transitionDocuments[1]))
              .Returns(_presentationFlags[1]);

            var mockLogger = new Mock<ILogger<GetCaseDocuments>>();

            _getCaseDocuments = new GetCaseDocuments(
                mockDocumentExtractionService.Object,
                mockDocumentToggleService.Object,
                mockTransitionDocumentMapper.Object,
                mockLogger.Object);
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
            _payload.CmsCaseId = 0;
            _mockDurableActivityContext.Setup(context => context.GetInput<GetCaseDocumentsActivityPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _getCaseDocuments.Run(_mockDurableActivityContext.Object));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Run_WhenAccessTokenIsNullOrWhitespace_ThrowsArgumentException(string cmsAuthValues)
        {
            _payload.CmsAuthValues = cmsAuthValues;
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
            _payload.CmsCaseUrn = caseUrn;
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
            var transitionDocuments = await _getCaseDocuments.Run(_mockDurableActivityContext.Object);

            transitionDocuments.Should().BeEquivalentTo(_transitionDocuments);
        }
    }
}
