using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using Common.Dto.Request;
using Common.Services.BlobStorageService.Contracts;
using Common.ValueObjects;
using Common.Wrappers.Contracts;
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
    private readonly IDocumentRedactionService _documentRedactionService;
    private readonly RedactPdfRequestDto _redactPdfRequest;
    private readonly Guid _correlationId;
    private readonly string _errorMessage;
    private readonly string _uploadFileName;
    private readonly string _redactionJsonUploadFileName;
    private readonly Stream _outputStream;
    private readonly MemoryStream _redactionJsonStream;

    public DocumentRedactionServiceTests()
    {
        var fixture = new Fixture();

        _mockBlobStorageService = new Mock<IPolarisBlobStorageService>();
        var mockUploadFileNameFactory = new Mock<IUploadFileNameFactory>();

        var mockRedactionProvider = new Mock<IRedactionProvider>();

        var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
        var mockLogger = new Mock<ILogger<DocumentRedactionService>>();

        _documentRedactionService = new DocumentRedactionService(
            _mockBlobStorageService.Object,
            mockUploadFileNameFactory.Object,
            mockRedactionProvider.Object,
            mockJsonConvertWrapper.Object,
            mockLogger.Object);

        _redactPdfRequest = fixture.Create<RedactPdfRequestDto>();
        _correlationId = fixture.Create<Guid>();
        _uploadFileName = fixture.Create<string>();
        _redactionJsonUploadFileName = fixture.Create<string>();

        var inputStream = new MemoryStream();
        _outputStream = new MemoryStream();
        _redactionJsonStream = new MemoryStream();

        _mockBlobStorageService.Setup(s => s.GetDocumentAsync(
                It.Is<string>((s) => s == _redactPdfRequest.FileName),
                It.Is<Guid>(g => g == _correlationId)))
            .ReturnsAsync(inputStream);

        mockUploadFileNameFactory.Setup(f => f.BuildUploadFileName(
            It.Is<string>(s => s == _redactPdfRequest.FileName)
        )).Returns(_uploadFileName);

        mockUploadFileNameFactory.Setup(f => f.BuildRedactionJsonFileName(
            It.Is<string>(s => s == _uploadFileName)
        )).Returns(_redactionJsonUploadFileName);

        mockRedactionProvider.Setup(s => s.Redact(
                It.Is<Stream>(s => s == inputStream),
                It.Is<RedactPdfRequestDto>(r => r == _redactPdfRequest),
                It.Is<Guid>(g => g == _correlationId)
            ))
            .Returns(_outputStream);

        mockJsonConvertWrapper.Setup(s => s.SerializeObjectToStream(
            It.Is<object>(o => o == _redactPdfRequest)
        )).Returns(_redactionJsonStream);

        _errorMessage = fixture.Create<string>();

        _mockBlobStorageService.Setup(s => s.UploadDocumentAsync(
            It.Is<Stream>(s => s == _outputStream),
            It.Is<string>(s => s == _uploadFileName),
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
            saveResult.RedactedDocumentName.Should().BeNull();
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
            saveResult.Message.Should().BeNull();
            saveResult.RedactedDocumentName.Should().Be(_uploadFileName);

            _mockBlobStorageService.Verify(s => s.UploadDocumentAsync(
                It.Is<Stream>(s => s == _outputStream),
                It.Is<string>(s => s == _uploadFileName),
                It.Is<string>(s => s == _redactPdfRequest.CaseId.ToString()),
                It.Is<PolarisDocumentId>(s => s == _redactPdfRequest.PolarisDocumentId),
                It.Is<string>(s => s == _redactPdfRequest.VersionId.ToString()),
                It.Is<Guid>(g => g == _correlationId))
            );

            _mockBlobStorageService.Verify(s => s.UploadRedactionJsonAsync(
                It.Is<Stream>(s => s == _redactionJsonStream),
                It.Is<string>(s => s == _redactionJsonUploadFileName),
                It.Is<Guid>(g => g == _correlationId))
            );
        }
    }
}
