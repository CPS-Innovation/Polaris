using System;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using Common.Dto.Response;
using Common.Dto.Tracker;
using coordinator.Durable.Activity;
using coordinator.Durable.Entity;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;
using coordinator.Functions.Orchestration.Functions.Document;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Durable.Orchestration
{
    public class RefreshDocumentOrchestratorTests
    {
        private Fixture _fixture;
        private readonly CaseDocumentOrchestrationPayload _payload;
        private readonly Mock<IDurableOrchestrationContext> _mockDurableOrchestrationContext;
        private readonly Mock<ICaseDurableEntity> _mockCaseEntity;
        private readonly RefreshDocumentOrchestrator _caseDocumentOrchestrator;

        public RefreshDocumentOrchestratorTests()
        {
            _fixture = new Fixture();
            var trackerCmsDocumentDto = _fixture.Create<DocumentDto>();
            var trackerPcdRequestDto = _fixture.Create<PcdRequestEntity>();
            var defendantsAndChargesListDto = _fixture.Create<DefendantsAndChargesEntity>();
            _payload = new CaseDocumentOrchestrationPayload
                (
                    _fixture.Create<string>(),
                    Guid.NewGuid(),
                    _fixture.Create<string>(),
                    _fixture.Create<int>(),
                    JsonSerializer.Serialize(trackerCmsDocumentDto),
                    JsonSerializer.Serialize(trackerPcdRequestDto),
                    JsonSerializer.Serialize(defendantsAndChargesListDto),
                    DocumentDeltaType.RequiresIndexing
                );

            var mockLogger = new Mock<ILogger<RefreshDocumentOrchestrator>>();
            _mockDurableOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockCaseEntity = new Mock<ICaseDurableEntity>();
            _mockCaseEntity.Setup(entity => entity.GetVersion()).ReturnsAsync(1);

            var extractTextResponse = _fixture.Create<ExtractTextResult>();

            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<ExtractTextResult>(nameof(ExtractText), _payload))
                .Returns(Task.FromResult(extractTextResponse));

            _mockDurableOrchestrationContext
                .Setup(context => context.GetInput<CaseDocumentOrchestrationPayload>())
                .Returns(_payload);

            _mockDurableOrchestrationContext
                .Setup(context => context.CreateEntityProxy<ICaseDurableEntity>(It.Is<EntityId>(e => e.EntityName == nameof(CaseDurableEntity).ToLower() && e.EntityKey == $"[{_payload.CmsCaseId}]")))
                .Returns(_mockCaseEntity.Object);

            _caseDocumentOrchestrator = new RefreshDocumentOrchestrator(mockLogger.Object);
        }

        [Fact]
        public async Task Run_ThrowsExceptionWhenPayloadIsNull()
        {
            _mockDurableOrchestrationContext
                .Setup(context => context.GetInput<CaseDocumentOrchestrationPayload>())
                .Returns((CaseDocumentOrchestrationPayload)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersPdfBlobName()
        {
            // Arrange
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<bool>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(true);

            // Act
            await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            // Assert
            _mockCaseEntity.Verify
                (
                    tracker =>
                    tracker.SetDocumentStatus
                    (
                        It.Is<(string, DocumentStatus, string)>
                        (
                            a =>
                                a.Item1 == _payload.CmsDocumentTracker.PolarisDocumentId.ToString() &&
                                a.Item2 == DocumentStatus.PdfUploadedToBlob &&
                                a.Item3 == _payload.BlobName
                        )
                    )
                );
        }

        [Fact]
        public async Task Run_Tracker_DoesNotIndexDocumentWithStatusNotEqualToRequiresIndexing()
        {
            // Arrange
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<bool>(It.Is<string>(s => s == nameof(GeneratePdf)), It.IsAny<object>()))
                .ReturnsAsync(true);

            var extractTextResult = _fixture.Create<ExtractTextResult>();
            extractTextResult.LineCount = -99;

            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<ExtractTextResult>(It.Is<string>(s => s == nameof(ExtractText)), It.IsAny<object>()))
                .ReturnsAsync(extractTextResult);

            _payload.DocumentDeltaType = DocumentDeltaType.RequiresPdfRefresh;

            // Act
            var result = await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            // Assert
            result.Should().BeOfType<RefreshDocumentResult>();
            result.OcrLineCount.Should().NotBe(extractTextResult.LineCount);
        }

        [Fact]
        public async Task Run_Tracker_DoesIndexDocumentWithStatusEqualToRequiresIndexing()
        {
            // Arrange
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<bool>(It.Is<string>(s => s == nameof(GeneratePdf)), It.IsAny<object>()))
                .ReturnsAsync(true);

            var extractTextResult = _fixture.Create<ExtractTextResult>();
            extractTextResult.LineCount = -99;

            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<ExtractTextResult>(It.Is<string>(s => s == nameof(ExtractText)), It.IsAny<object>()))
                .ReturnsAsync(extractTextResult);

            _payload.DocumentDeltaType = DocumentDeltaType.RequiresIndexing;

            // Act
            var result = await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            // Assert
            result.Should().BeOfType<RefreshDocumentResult>();
            result.OcrLineCount.Should().Be(extractTextResult.LineCount);
        }

        [Fact]
        public async Task Run_Tracker_DoesNotRegistersPdfBlobNameOrIndexIfCallPdfGeneratorAsyncReturnsFalse()
        {
            // Arrange
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<bool>(It.Is<string>(s => s == nameof(GeneratePdf)), It.IsAny<object>()))
                .ReturnsAsync(false);

            // Act
            await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            // Assert
            _mockCaseEntity.Verify
            (
                tracker => tracker.SetDocumentStatus
                (
                    It.Is<(string, DocumentStatus, string)>
                    (
                        a => a.Item1 == _payload.CmsDocumentTracker.PolarisDocumentId.ToString() &&
                            a.Item2 == DocumentStatus.UnableToConvertToPdf &&
                            a.Item3 == null
                    )
                )
            );
            _mockCaseEntity.Verify
                (
                    tracker => tracker.SetDocumentStatus
                    (
                        It.Is<(string, DocumentStatus, string)>
                        (
                            a => a.Item2 == DocumentStatus.PdfUploadedToBlob
                        )
                    ),
                    Times.Never
                );
            _mockCaseEntity.Verify
            (
                tracker => tracker.SetDocumentStatus
                (
                    It.Is<(string, DocumentStatus, string)>
                    (
                        a => a.Item2 == DocumentStatus.Indexed
                    )
                ),
                Times.Never
            );
        }

        [Fact]
        public async Task Run_Tracker_RegistersIndexed_WhenNotAlreadyProcessed()
        {
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<bool>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(true);


            await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockCaseEntity.Verify
                (
                    tracker =>
                    tracker.SetDocumentStatus
                    (
                        It.Is<(string, DocumentStatus, string)>
                        (
                            a =>
                                a.Item1 == _payload.PolarisDocumentId.ToString() &&
                                a.Item2 == DocumentStatus.Indexed &&
                                a.Item3 == _payload.BlobName
                        )
                    )
                );
        }

        [Fact]
        public async Task Run_WhenDocumentEvaluation_EqualsAcquireDocument_AndSearchIndexUpdated_RegistersUnexpectedDocumentFailureWhenCallToGeneratePdfReturnsNonOkResponse()
        {
            // Arrange
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync(It.IsAny<string>(), It.IsAny<CaseDocumentOrchestrationPayload>()))
                .Throws(new Exception());

            try
            {
                // Act
                await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                // Assert
                _mockCaseEntity.Verify
                    (
                        tracker =>
                        tracker.SetDocumentStatus
                        (
                            It.Is<(string, DocumentStatus, string)>
                            (
                                a =>
                                    a.Item1 == _payload.PolarisDocumentId.ToString() &&
                                    a.Item2 == DocumentStatus.UnableToConvertToPdf
                            )
                        )
                    );
            }
        }
    }
}
