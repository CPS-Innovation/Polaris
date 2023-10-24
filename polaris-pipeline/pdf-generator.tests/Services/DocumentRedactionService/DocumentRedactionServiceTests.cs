using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using Common.Dto.Request;
using Common.Services.BlobStorageService.Contracts;
using Common.ValueObjects;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using pdf_generator.Services.DocumentRedaction;
using Xunit;

namespace pdf_generator.tests.Services.DocumentRedaction;

public class DocumentRedactionServiceTests
{
    private readonly Mock<IPolarisBlobStorageService> _mockBlobStorageService;
    private readonly Mock<IRedactionProvider> _mockRedactionProvider;
    private readonly IDocumentRedactionService _documentRedactionService;
    private readonly RedactPdfRequestDto _redactPdfRequest;
    private readonly Guid _correlationId;
    private readonly string _errorMessage;

    public DocumentRedactionServiceTests()
    {
        var fixture = new Fixture();

        _mockBlobStorageService = new Mock<IPolarisBlobStorageService>();
        _mockRedactionProvider = new Mock<IRedactionProvider>();
        var mockLogger = new Mock<ILogger<DocumentRedactionService>>();

        _documentRedactionService = new DocumentRedactionService(
            _mockBlobStorageService.Object,
            _mockRedactionProvider.Object,
            mockLogger.Object);

        _redactPdfRequest = fixture.Create<RedactPdfRequestDto>();
        _correlationId = fixture.Create<Guid>();
        var inputStream = new MemoryStream();
        var outputStream = new MemoryStream();

        _mockBlobStorageService.Setup(s => s.GetDocumentAsync(
                It.Is<string>((s) => s == _redactPdfRequest.FileName),
                It.Is<Guid>(g => g == _correlationId)))
            .ReturnsAsync(inputStream);

        _mockRedactionProvider.Setup(s => s.Redact(
                It.Is<Stream>(s => s == inputStream),
                It.Is<RedactPdfRequestDto>(r => r == _redactPdfRequest),
                It.Is<Guid>(g => g == _correlationId)
            ))
            .Returns(outputStream);

        _errorMessage = fixture.Create<string>();

        _mockBlobStorageService.Setup(s => s.UploadDocumentAsync(
            It.Is<Stream>(s => s == outputStream),
            It.IsAny<string>(),
            It.Is<string>(s => s == _redactPdfRequest.CaseId.ToString()),
            It.Is<PolarisDocumentId>(s => s == _redactPdfRequest.PolarisDocumentId),
            It.Is<string>(s => s == _redactPdfRequest.VersionId.ToString()),
            It.Is<Guid>(g => g == _correlationId)))
        .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task DocumentRedactionService_RedactPdfAsync_ReturnsAFailureResponseIfUploadDocumentAsyncThrows()
    {
        // Arrange
        _mockBlobStorageService
        .Setup(s => s.UploadDocumentAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<PolarisDocumentId>(),
            It.IsAny<string>(),
            It.IsAny<Guid>()))
        .ThrowsAsync(new Exception(_errorMessage));

        // Act
        var saveResult = await _documentRedactionService.RedactPdfAsync(_redactPdfRequest, _correlationId);

        // Assert
        using (new AssertionScope())
        {
            saveResult.Succeeded.Should().BeFalse();
            saveResult.Message.Should().Be(_errorMessage);
        }
    }

    [Fact]
    public async Task DocumentRedactionService_RedactPdfAsync_ReturnsASuccessResponse()
    {
        // Act
        var saveResult = await _documentRedactionService.RedactPdfAsync(_redactPdfRequest, _correlationId);

        // Assert
        using (new AssertionScope())
        {
            saveResult.Succeeded.Should().BeTrue();
            saveResult.Message.Should().Be(_errorMessage);
        }
    }
}
