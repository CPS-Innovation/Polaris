﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Common.Constants;
using Common.Domain.Extensions;
using Common.Domain.Responses;
using Common.Wrappers;
using coordinator.Domain;
using coordinator.Domain.Tracker;
using coordinator.Functions.ActivityFunctions;
using coordinator.Functions.SubOrchestrators;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.SubOrchestrators
{
    public class CaseDocumentOrchestratorTests
    {
        private readonly CaseDocumentOrchestrationPayload _payload;
        private readonly DurableHttpRequest _evaluateDocumentDurableRequest;
        private readonly DurableHttpRequest _generatePdfDurableRequest;
        private readonly string _content;
        private readonly GeneratePdfResponse _pdfResponse;
        private readonly EvaluateDocumentResponse _evaluateDocumentResponse;

        private readonly Mock<IDurableOrchestrationContext> _mockDurableOrchestrationContext;
        private readonly Mock<ITracker> _mockTracker;

        private readonly CaseDocumentOrchestrator _caseDocumentOrchestrator;

        public CaseDocumentOrchestratorTests()
        {
            var fixture = new Fixture();
            _payload = fixture.Create<CaseDocumentOrchestrationPayload>();
            _evaluateDocumentDurableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.google.co.uk/evaluateDocument"));
            _generatePdfDurableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.google.co.uk/generatePdf"));
            var updateSearchIndexDurableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.google.co.uk/updateSearchIndex"));
            var textExtractorDurableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.google.co.uk/textExtractor"));
            _content = fixture.Create<string>();
            var durableResponse = new DurableHttpResponse(HttpStatusCode.OK, content: _content);
            _pdfResponse = fixture.Create<GeneratePdfResponse>();
            
            var mockLogger = new Mock<ILogger<CaseDocumentOrchestrator>>();
            _mockDurableOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockTracker = new Mock<ITracker>();
            
            _evaluateDocumentResponse = fixture.Create<EvaluateDocumentResponse>();

            _mockDurableOrchestrationContext.Setup(context => context.GetInput<CaseDocumentOrchestrationPayload>()).Returns(_payload);
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<DurableHttpRequest>(
                nameof(CreateGeneratePdfHttpRequest),
                It.Is<GeneratePdfHttpRequestActivityPayload>(p => p.CaseId == _payload.CaseId && p.DocumentId == _payload.DocumentId && p.FileName == _payload.FileName)))
                    .ReturnsAsync(_generatePdfDurableRequest);
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<DurableHttpRequest>(
                nameof(CreateTextExtractorHttpRequest),
                It.Is<TextExtractorHttpRequestActivityPayload>(p => p.CaseId == _payload.CaseId && p.DocumentId == _payload.DocumentId && p.BlobName == _pdfResponse.BlobName)))
                    .ReturnsAsync(textExtractorDurableRequest);
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_generatePdfDurableRequest)).ReturnsAsync(durableResponse);
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(textExtractorDurableRequest)).ReturnsAsync(durableResponse);
            
            //set default activity responses
            _evaluateDocumentResponse.EvaluationResult = DocumentEvaluationResult.AcquireDocument;
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_evaluateDocumentDurableRequest)).ReturnsAsync(new DurableHttpResponse(HttpStatusCode.OK, content: _evaluateDocumentResponse.ToJson()));
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(updateSearchIndexDurableRequest)).ReturnsAsync(new DurableHttpResponse(HttpStatusCode.OK, content: _content));
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_generatePdfDurableRequest)).ReturnsAsync(new DurableHttpResponse(HttpStatusCode.OK, content: _pdfResponse.ToJson()));
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(textExtractorDurableRequest)).ReturnsAsync(new DurableHttpResponse(HttpStatusCode.OK, content: _content));

            _mockDurableOrchestrationContext.Setup(context => context.CreateEntityProxy<ITracker>(It.Is<EntityId>(e => e.EntityName == nameof(Tracker).ToLower() && e.EntityKey == _payload.CaseId.ToString())))
                .Returns(_mockTracker.Object);
            
            _caseDocumentOrchestrator = new CaseDocumentOrchestrator(new JsonConvertWrapper(), mockLogger.Object);
        }

        [Fact]
        public async Task Run_ThrowsExceptionWhenPayloadIsNull()
        {
            _mockDurableOrchestrationContext.Setup(context => context.GetInput<CaseDocumentOrchestrationPayload>()).Returns(default(CaseDocumentOrchestrationPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersPdfBlobName()
        {
            _pdfResponse.AlreadyProcessed = false;
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_generatePdfDurableRequest)).ReturnsAsync(new DurableHttpResponse(HttpStatusCode.OK, content: _pdfResponse.ToJson()));
            
            await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.RegisterPdfBlobName(It.Is<RegisterPdfBlobNameArg>(a => a.DocumentId == _payload.DocumentId && a.BlobName == _pdfResponse.BlobName)));
        }

        [Fact]
        public async Task Run_Tracker_RegistersIndexed_WhenNotAlreadyProcessed()
        {
            _pdfResponse.AlreadyProcessed = false;
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_generatePdfDurableRequest)).ReturnsAsync(new DurableHttpResponse(HttpStatusCode.OK, content: _pdfResponse.ToJson()));
            
            await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.RegisterIndexed(_payload.DocumentId));
        }
        
        [Fact]
        public async Task Run_ThrowsExceptionWhenCallToGeneratePdfReturnsNonOkResponse()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_generatePdfDurableRequest))
                .ReturnsAsync(new DurableHttpResponse(HttpStatusCode.InternalServerError, content: _content));

            await Assert.ThrowsAsync<HttpRequestException>(() => _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersDocumentNotFoundInDDEIWhenNotFoundStatusCodeReturned()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_generatePdfDurableRequest))
                .ReturnsAsync(new DurableHttpResponse(HttpStatusCode.NotFound, content: _content));

            try
            {
                await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                _mockTracker.Verify(tracker => tracker.RegisterDocumentNotFoundInDDEI(_payload.DocumentId));
            }
        }

        [Fact]
        public async Task Run_Tracker_RegistersFailedToConvertToPdfWhenNotFoundStatusCodeReturned()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_generatePdfDurableRequest))
                .ReturnsAsync(new DurableHttpResponse(HttpStatusCode.NotImplemented, content: _content));
  
            try
            {
                await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                _mockTracker.Verify(tracker => tracker.RegisterUnableToConvertDocumentToPdf(_payload.DocumentId));
            }
        }

        [Fact]
        public async Task Run_WhenDocumentEvaluation_EqualsAcquireDocument_AndSearchIndexUpdated_RegistersUnexpectedDocumentFailureWhenCallToGeneratePdfReturnsNonOkResponse()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_generatePdfDurableRequest))
                .ReturnsAsync(new DurableHttpResponse(HttpStatusCode.InternalServerError, content: _content));

            try
            {
                await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                _mockTracker.Verify(tracker => tracker.RegisterUnexpectedPdfDocumentFailure(_payload.DocumentId));
            }
        }

        [Fact]
        public async Task Run_RegistersAsIndexed_WhenDocumentEvaluation_EqualsDocumentUnchanged()
        {
            _evaluateDocumentResponse.EvaluationResult = DocumentEvaluationResult.DocumentUnchanged;
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_evaluateDocumentDurableRequest)).ReturnsAsync(new DurableHttpResponse(HttpStatusCode.OK, content: _evaluateDocumentResponse.ToJson()));
            
            try
            {
                await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                _mockTracker.Verify(tracker => tracker.RegisterBlobAlreadyProcessed(It.IsAny<RegisterPdfBlobNameArg>()));
            }
        }
    }
}
