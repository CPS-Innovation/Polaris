using System;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using Common.Dto.Response;
using Common.Dto.Tracker;
using coordinator.Domain;
using coordinator.Domain.Entity;
using coordinator.Functions.ActivityFunctions.Document;
using coordinator.Functions.DurableEntity.Entity;
using coordinator.Functions.DurableEntity.Entity.Contract;
using coordinator.Functions.Orchestration.Functions.Document;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.SubOrchestrators
{
    public class RefreshDocumentOrchestratorTests
    {
        private readonly CaseDocumentOrchestrationPayload _payload;
        private readonly Mock<IDurableOrchestrationContext> _mockDurableOrchestrationContext;
        private readonly Mock<ICaseDurableEntity> _mockCaseEntity;
        private readonly RefreshDocumentOrchestrator _caseDocumentOrchestrator;

        public RefreshDocumentOrchestratorTests()
        {
            var fixture = new Fixture();
            var trackerCmsDocumentDto = fixture.Create<DocumentDto>();
            var trackerPcdRequestDto = fixture.Create<PcdRequestEntity>();
            var defendantsAndChargesListDto = fixture.Create<DefendantsAndChargesEntity>();
            _payload = new CaseDocumentOrchestrationPayload
                (
                    fixture.Create<string>(),
                    Guid.NewGuid(),
                    fixture.Create<string>(),
                    fixture.Create<long>(),
                    JsonSerializer.Serialize(trackerCmsDocumentDto),
                    JsonSerializer.Serialize(trackerPcdRequestDto),
                    JsonSerializer.Serialize(defendantsAndChargesListDto),
                    DocumentDeltaType.RequiresIndexing
                );

            var mockLogger = new Mock<ILogger<RefreshDocumentOrchestrator>>();
            _mockDurableOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockCaseEntity = new Mock<ICaseDurableEntity>();
            _mockCaseEntity.Setup(entity => entity.GetVersion()).ReturnsAsync(1);

            var extractTextResponse = fixture.Create<ExtractTextResult>();

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
