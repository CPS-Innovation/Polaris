using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using coordinator.Durable.Activity;
using Common.Dto.Response.Document;
using Common.Dto.Response.Document.FeatureFlags;
using Ddei;
using Common.Services.DocumentToggle;
using Common.Dto.Response.Case;
using Microsoft.Extensions.Configuration;
using coordinator.Durable.Payloads;
using Ddei.Factories;
using Ddei.Domain.CaseData.Args.Core;

namespace coordinator.tests.Durable.Activity
{
    public class GetCaseDocumentsTests
    {
        private readonly CaseDto _case;
        private readonly CmsDocumentDto[] _caseDocuments;
        private readonly PresentationFlagsDto[] _presentationFlags;
        private readonly CasePayload _payload;
        private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;
        private readonly GetCaseDocuments _getCaseDocuments;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public GetCaseDocumentsTests()
        {
            var fixture = new Fixture();
            _payload = fixture.Create<CasePayload>();
            _case = fixture.Create<CaseDto>();
            _caseDocuments = new[] {
              fixture.Create<CmsDocumentDto>(),
              fixture.Create<CmsDocumentDto>()
            };

            _presentationFlags = new[] {
              fixture.Create<PresentationFlagsDto>(),
              fixture.Create<PresentationFlagsDto>()
            };

            var mockDdeiClient = new Mock<IDdeiClient>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockConfiguration = new Mock<IConfiguration>();

            _mockDurableActivityContext
                .Setup(context => context.GetInput<CasePayload>())
                .Returns(_payload);

            mockDdeiClient
                .Setup(client => client.GetCaseAsync(It.IsAny<DdeiCaseIdentifiersArgDto>()))
                .ReturnsAsync(_case);

            var mockDdeiCaseIdentifiersArgDto = fixture.Create<DdeiCaseIdentifiersArgDto>();

            var mockDdeiArgFactory = new Mock<IDdeiArgFactory>();
            mockDdeiArgFactory
                .Setup(factory => factory.CreateCaseIdentifiersArg(_payload.CmsAuthValues, _payload.CorrelationId, _payload.Urn, _payload.CaseId))
                .Returns(mockDdeiCaseIdentifiersArgDto);

            mockDdeiClient
                .Setup(client => client.ListDocumentsAsync(mockDdeiCaseIdentifiersArgDto))
                .ReturnsAsync(_caseDocuments);

            var mockDocumentToggleService = new Mock<IDocumentToggleService>();
            mockDocumentToggleService
              .Setup(service => service.GetDocumentPresentationFlags(_caseDocuments[0]))
              .Returns(_presentationFlags[0]);
            mockDocumentToggleService
              .Setup(service => service.GetDocumentPresentationFlags(_caseDocuments[1]))
              .Returns(_presentationFlags[1]);

            var mockLogger = new Mock<ILogger<GetCaseDocuments>>();

            _getCaseDocuments = new GetCaseDocuments(
                mockDdeiClient.Object,
                mockDdeiArgFactory.Object
                // mockDocumentToggleService.Object,
                // mockLogger.Object,
                // _mockConfiguration.Object
                );
        }

        [Fact]
        public async Task Run_WhenCaseIdIsZero_ThrowsArgumentException()
        {
            _payload.CaseId = 0;
            _mockDurableActivityContext.Setup(context => context.GetInput<CasePayload>())
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
            _mockDurableActivityContext.Setup(context => context.GetInput<CasePayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _getCaseDocuments.Run(_mockDurableActivityContext.Object));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Run_WhenCaseUrnIsNullOrWhitespace_ThrowsArgumentException(string caseUrn)
        {
            _payload.Urn = caseUrn;
            _mockDurableActivityContext.Setup(context => context.GetInput<CasePayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _getCaseDocuments.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_WhenCorrelationIdIsEmpty_ThrowsArgumentException()
        {
            _payload.CorrelationId = Guid.Empty;
            _mockDurableActivityContext.Setup(context => context.GetInput<CasePayload>())
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
