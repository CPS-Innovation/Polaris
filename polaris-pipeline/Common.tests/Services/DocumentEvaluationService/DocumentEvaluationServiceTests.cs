using AutoFixture;
using Common.Constants;
using Common.Domain.BlobStorage;
using Common.Dto.Document;
using Common.Dto.Request;
using Common.Services.BlobStorageService.Contracts;
using Common.Services.DocumentEvaluation.Contracts;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Common.tests.Services.DocumentEvaluationService;

public class DocumentEvaluationServiceTests
{
    private readonly Mock<IPolarisBlobStorageService> _mockBlobStorageService;
    private readonly Fixture _fixture;
    private readonly Guid _correlationId;

    private readonly IDocumentEvaluationService _documentEvaluationService;

    public DocumentEvaluationServiceTests()
    {
        _fixture = new Fixture();
        _mockBlobStorageService = new Mock<IPolarisBlobStorageService>();
        var mockLogger = new Mock<ILogger<Common.Services.DocumentEvaluation.DocumentEvaluationService>>();

        _correlationId = Guid.NewGuid();

        var incomingDocument = _fixture.Create<DocumentDto>();

        _documentEvaluationService = new Common.Services.DocumentEvaluation.DocumentEvaluationService(_mockBlobStorageService.Object, mockLogger.Object);

        _mockBlobStorageService.Setup(s => s.RemoveDocumentAsync(It.IsAny<string>(), _correlationId)).ReturnsAsync(true);

        var convertedBlob = new BlobSearchResult {BlobName = incomingDocument.FileName, VersionId = incomingDocument.VersionId};
        
        _mockBlobStorageService.Setup(x => x.FindBlobsByPrefixAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync(new List<BlobSearchResult> { convertedBlob });
    }

    [Fact]
    public async Task EvaluateDocumentAsync_WhenDocumentIsNotFoundInBlobStorage_ShouldAcquireDocument()
    {
        var request = _fixture.Create<EvaluateDocumentRequestDto>();
        _mockBlobStorageService.Setup(x => x.FindBlobsByPrefixAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync(new List<BlobSearchResult>());
        
        var result = await _documentEvaluationService.EvaluateDocumentAsync(request, _correlationId);
        
        using (new AssertionScope())
        {
            result.CaseId.Should().Be(request.CaseId);
            result.DocumentId.Should().Be(request.DocumentId);
            result.EvaluationResult.Should().Be(DocumentEvaluationResult.AcquireDocument);
        }
    }
    
    [Fact]
    public async Task EvaluateDocumentAsync_WhenDocumentIsMatchedExactlyToBlobStorage_ShouldLeaveTheDocumentUnchanged()
    {
        var request = _fixture.Create<EvaluateDocumentRequestDto>();
        var storedDocument = new BlobSearchResult
        {
            BlobName = request.ProposedBlobName,
            VersionId = request.VersionId
        };
        
        _mockBlobStorageService.Setup(s => s.FindBlobsByPrefixAsync(It.IsAny<string>(), _correlationId))
            .ReturnsAsync(new List<BlobSearchResult> { storedDocument });

        var result = await _documentEvaluationService.EvaluateDocumentAsync(request, _correlationId);
        
        using (new AssertionScope())
        {
            result.CaseId.Should().Be(request.CaseId);
            result.DocumentId.Should().Be(request.DocumentId);
            result.EvaluationResult.Should().Be(DocumentEvaluationResult.DocumentUnchanged);
        }
    }
    
    [Fact]
    public async Task EvaluateDocumentAsync_WhenDocumentIsNotMatchedExactlyToBlobStorage_ByVersionId_ShouldAcquireTheNewDocument_AndUpdateTheSearchIndexToRemoveTheOld()
    {
        var request = _fixture.Create<EvaluateDocumentRequestDto>();
        var storedDocument = new BlobSearchResult
        {
            BlobName = _fixture.Create<string>(),
            VersionId = _fixture.Create<long>()
        };
        
        _mockBlobStorageService.Setup(s => s.FindBlobsByPrefixAsync(It.IsAny<string>(), _correlationId))
            .ReturnsAsync(new List<BlobSearchResult> { storedDocument });

        var result = await _documentEvaluationService.EvaluateDocumentAsync(request, _correlationId);
        
        using (new AssertionScope())
        {
            result.CaseId.Should().Be(request.CaseId);
            result.DocumentId.Should().Be(request.DocumentId);
            result.EvaluationResult.Should().Be(DocumentEvaluationResult.AcquireDocument);
        }
    }
}