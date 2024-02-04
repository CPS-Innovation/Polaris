using System;
using System.Threading.Tasks;
using AutoFixture;
using coordinator.Domain;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Xunit;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;
using DdeiClient.Services.Contracts;
using coordinator.Services.DocumentToggle;
using Ddei.Domain.CaseData.Args;
using Common.Dto.Case;
using coordinator.Functions.Durable.ActivityFunctions;

namespace coordinator.tests.Functions.ActivityFunctions
{
    public class GetCaseDocumentsTests
    {
        private readonly CaseDto _case;
        private readonly CmsDocumentDto[] _caseDocuments;
        private readonly PresentationFlagsDto[] _presentationFlags;
        private readonly GetCaseDocumentsActivityPayload _payload;
        private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;
        private readonly GetCaseDocuments _getCaseDocuments;

        public GetCaseDocumentsTests()
        {
            var fixture = new Fixture();
            _payload = fixture.Create<GetCaseDocumentsActivityPayload>();
            _case = fixture.Create<CaseDto>();
            _caseDocuments = new[] {
              fixture.Create<CmsDocumentDto>(),
              fixture.Create<CmsDocumentDto>()
            };

            _presentationFlags = new[] {
              fixture.Create<PresentationFlagsDto>(),
              fixture.Create<PresentationFlagsDto>()
            };

            var mockDocumentExtractionService = new Mock<IDdeiClient>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDurableActivityContext
                .Setup(context => context.GetInput<GetCaseDocumentsActivityPayload>())
                .Returns(_payload);

            mockDocumentExtractionService
                .Setup(client => client.GetCaseAsync(It.IsAny<DdeiCmsCaseArgDto>()))
                .ReturnsAsync(_case);
            mockDocumentExtractionService
                .Setup(client => client.ListDocumentsAsync(_payload.CmsCaseUrn, _payload.CmsCaseId.ToString(), _payload.CmsAuthValues, _payload.CorrelationId))
                .ReturnsAsync(_caseDocuments);

            var mockDocumentToggleService = new Mock<IDocumentToggleService>();
            mockDocumentToggleService
              .Setup(service => service.GetDocumentPresentationFlags(_caseDocuments[0]))
              .Returns(_presentationFlags[0]);
            mockDocumentToggleService
              .Setup(service => service.GetDocumentPresentationFlags(_caseDocuments[1]))
              .Returns(_presentationFlags[1]);


            _getCaseDocuments = new GetCaseDocuments(
                mockDocumentExtractionService.Object,
                mockDocumentToggleService.Object);
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
            var (documents, _, _) = await _getCaseDocuments.Run(_mockDurableActivityContext.Object);

            documents.Should().BeEquivalentTo(_caseDocuments);
        }
    }
}
