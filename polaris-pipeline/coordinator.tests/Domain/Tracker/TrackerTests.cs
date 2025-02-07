using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Common.Wrappers;
using coordinator.Functions;
using Common.Dto.Response.Documents;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using coordinator.Durable.Payloads.Domain;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using coordinator.Domain;
using Common.Constants;
using coordinator.Services;

namespace coordinator.tests.Domain.Response.Documents
{
    public class TrackerTests
    {
        private readonly Fixture _fixture;
        private readonly List<CmsDocumentEntity> _trackerCmsDocuments;
        private readonly List<PcdRequestEntity> _trackerPcdRequests;
        private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly Guid _correlationId;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly Mock<IStateStorageService> _mockStateStorageService;
        private readonly Mock<ILogger<GetTracker>> _mockLogger;
        private readonly CaseDurableEntityState _caseDurableEntityState;
        private readonly CaseDurableEntityDocumentsState _caseDurableEntityDocumentsState;
        private readonly GetTracker _trackerStatus;

        public TrackerTests()
        {
            _fixture = new Fixture();
            _correlationId = _fixture.Create<Guid>();
            _caseDurableEntityState = _fixture.Create<CaseDurableEntityState>();

            // (At least on a mac) this test suite crashes unless we control the format of CmsDocumentEntity.CmsOriginalFileName so that it
            //  matches the regex attribute that decorates it.
            _fixture.Customize<CmsDocumentEntity>(c =>
                c.With(doc => doc.CmsOriginalFileName, $"{_fixture.Create<string>()}.{_fixture.Create<string>()[..3]}"));
            _trackerCmsDocuments = _fixture.Create<List<CmsDocumentEntity>>();

            _trackerPcdRequests = _fixture.Create<List<PcdRequestEntity>>();
            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<int>();

            _caseDurableEntityDocumentsState = new CaseDurableEntityDocumentsState
            {
                CmsDocuments = _trackerCmsDocuments,
                PcdRequests = _trackerPcdRequests,
            };

            _jsonConvertWrapper = _fixture.Create<JsonConvertWrapper>();

            _mockLogger = new Mock<ILogger<GetTracker>>();
            _mockStateStorageService = new Mock<IStateStorageService>();
            _mockStateStorageService.Setup(s => s.GetStateAsync(_caseId))
                .ReturnsAsync(_caseDurableEntityState);
            _mockStateStorageService.Setup(s => s.GetDurableEntityDocumentsStateAsync(_caseId))
                .ReturnsAsync(_caseDurableEntityDocumentsState);

            _trackerStatus = new GetTracker(_jsonConvertWrapper, new CaseDurableEntityMapper(), _mockStateStorageService.Object, _mockLogger.Object);
        }

        [Fact]
        public void Tracker_Serialised_And_Deserialises()
        {
            var serialisedTracker = JsonSerializer.Serialize(_caseDurableEntityDocumentsState);
            var deserialisedTracker = JsonSerializer.Deserialize<CaseDurableEntityDocumentsState>(serialisedTracker);

            _caseDurableEntityDocumentsState.CmsDocuments[0].DocumentId.Should().Be(deserialisedTracker.CmsDocuments[0].DocumentId);
            _caseDurableEntityDocumentsState.PcdRequests[0].DocumentId.Should().Be(deserialisedTracker.PcdRequests[0].DocumentId);
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsOK()
        {
            // Arrange
            var message = new DefaultHttpContext().Request;
            message.Headers.Append(HttpHeaderKeys.CorrelationId, _correlationId.ToString());

            // Act
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId);

            // Assert
            response.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsTrackerDto()
        {
            var message = new DefaultHttpContext().Request;
            message.Headers.Append(HttpHeaderKeys.CorrelationId, _correlationId.ToString());
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId);

            var okObjectResult = response as OkObjectResult;

            okObjectResult?.Value.Should().BeOfType<TrackerDto>();
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsNotFoundIfEntityNotFound()
        {
            // Arrange
            _mockStateStorageService.Setup(s => s.GetStateAsync(_caseId))
                .ThrowsAsync(new Exception());

            var message = new DefaultHttpContext().Request;
            message.Headers.Append(HttpHeaderKeys.CorrelationId, _correlationId.ToString());

            // Act
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId);

            // Assert
            response.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}