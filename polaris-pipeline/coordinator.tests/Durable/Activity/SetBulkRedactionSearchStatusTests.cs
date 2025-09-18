using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Dto.Request.Redaction;
using Common.Dto.Response;
using coordinator.Domain;
using coordinator.Durable.Activity;
using coordinator.Durable.Payloads;
using coordinator.Services;
using Fare;
using Moq;
using Xunit;

namespace coordinator.tests.Durable.Activity;

public class SetBulkRedactionSearchStatusTests
{
    private readonly Mock<IStateStorageService> _stateStorageServiceMock;
    private readonly SetBulkRedactionSearchStatus _setBulkRedactionSearchStatus;
    public SetBulkRedactionSearchStatusTests()
    {
        _stateStorageServiceMock = new Mock<IStateStorageService>();
        _setBulkRedactionSearchStatus = new SetBulkRedactionSearchStatus(_stateStorageServiceMock.Object);
    }

    [Fact]
    public async Task Run_StatusIsGeneratingOcrDocument_ShouldUpdateSearchTextAndUpdatedAt()
    {
        //arrange
        var payload = new BulkRedactionSearchStatusPayload()
        {
            SearchText = "searchText",
            UpdatedAt = DateTime.Today,
            Status = BulkRedactionSearchStatus.GeneratingOcrDocument
        };
        var state = new BulkRedactionSearchEntityState();
        _stateStorageServiceMock.Setup(s => s.GetBulkRedactionSearchStateAsync(payload.CaseId, payload.DocumentId, payload.VersionId, payload.SearchText)).ReturnsAsync(state);
        _stateStorageServiceMock.Setup(s => s.UpdateBulkRedactionSearchStateAsync(state)).ReturnsAsync(true);

        //act
        var result = await _setBulkRedactionSearchStatus.Run(payload);

        //assert
        Assert.Equal(payload.SearchText, state.SearchTerm);
        Assert.Equal(payload.UpdatedAt, state.UpdatedAt);
        Assert.Equal(payload.Status, state.Status);
    }
    
    [Fact]
    public async Task Run_StatusIsSearchingDocument_ShouldUpdateOcrDocumentGeneratedAtAndUpdatedAt()
    {
        //arrange
        var payload = new BulkRedactionSearchStatusPayload()
        {
            OcrDocumentGeneratedAt = DateTime.Today.AddHours(1),
            UpdatedAt = DateTime.Today,
            Status = BulkRedactionSearchStatus.SearchingDocument
        };
        var state = new BulkRedactionSearchEntityState();
        _stateStorageServiceMock.Setup(s => s.GetBulkRedactionSearchStateAsync(payload.CaseId, payload.DocumentId, payload.VersionId, payload.SearchText)).ReturnsAsync(state);
        _stateStorageServiceMock.Setup(s => s.UpdateBulkRedactionSearchStateAsync(state)).ReturnsAsync(true);

        //act
        var result = await _setBulkRedactionSearchStatus.Run(payload);

        //assert
        Assert.Equal(payload.OcrDocumentGeneratedAt, state.OcrDocumentGeneratedAt);
        Assert.Equal(payload.UpdatedAt, state.UpdatedAt);
        Assert.Equal(payload.Status, state.Status);
    }
    
    [Fact]
    public async Task Run_StatusIsCompleted_ShouldUpdateStateToCompleted()
    {
        //arrange
        var payload = new BulkRedactionSearchStatusPayload()
        {
            DocumentSearchCompletedAt = DateTime.Today.AddHours(1),
            CompletedAt = DateTime.Today.AddHours(2),
            UpdatedAt = DateTime.Today,
            Status = BulkRedactionSearchStatus.Completed,
            RedactionDefinitions = new List<RedactionDefinitionDto>()
        };
        var state = new BulkRedactionSearchEntityState();
        _stateStorageServiceMock.Setup(s => s.GetBulkRedactionSearchStateAsync(payload.CaseId, payload.DocumentId, payload.VersionId, payload.SearchText)).ReturnsAsync(state);
        _stateStorageServiceMock.Setup(s => s.UpdateBulkRedactionSearchStateAsync(state)).ReturnsAsync(true);

        //act
        var result = await _setBulkRedactionSearchStatus.Run(payload);

        //assert
        Assert.Equal(payload.DocumentSearchCompletedAt, state.DocumentSearchCompletedAt);
        Assert.Equal(payload.CompletedAt, state.CompletedAt);
        Assert.Equal(payload.UpdatedAt, state.UpdatedAt);
        Assert.Equal(payload.Status, state.Status);
        Assert.Equal(payload.RedactionDefinitions, state.RedactionDefinitions);
    }
    
    [Fact]
    public async Task Run_StatusIsFailed_ShouldUpdateStateToCompleted()
    {
        //arrange
        var payload = new BulkRedactionSearchStatusPayload()
        {
            FailedAt = DateTime.Today.AddHours(1),
            FailureReason = "failureReason",
            Status = BulkRedactionSearchStatus.Failed,
        };
        var state = new BulkRedactionSearchEntityState();
        _stateStorageServiceMock.Setup(s => s.GetBulkRedactionSearchStateAsync(payload.CaseId, payload.DocumentId, payload.VersionId, payload.SearchText)).ReturnsAsync(state);
        _stateStorageServiceMock.Setup(s => s.UpdateBulkRedactionSearchStateAsync(state)).ReturnsAsync(true);

        //act
        var result = await _setBulkRedactionSearchStatus.Run(payload);

        //assert
        Assert.Equal(payload.FailedAt, state.FailedAt);
        Assert.Equal(payload.FailureReason, state.FailureReason);
        Assert.Equal(payload.UpdatedAt, state.UpdatedAt);
        Assert.Equal(payload.Status, state.Status);
    }
}