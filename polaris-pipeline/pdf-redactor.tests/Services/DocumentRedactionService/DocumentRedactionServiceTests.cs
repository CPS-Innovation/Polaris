using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using Common.Dto.Request;
using Common.Services.BlobStorageService;
using Common.ValueObjects;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using pdf_redactor.Services.DocumentRedaction;
using Xunit;

namespace pdf_redactor.tests.Services.DocumentRedaction;

public class DocumentRedactionServiceTests
{
    private readonly IDocumentRedactionService _documentRedactionService;
    private readonly RedactPdfRequestDto _redactPdfRequest;
    private readonly RedactPdfRequestWithDocumentDto _redactPdfRequestWithDocument;

    private readonly Guid _correlationId;
    private readonly string _caseId;
    private readonly PolarisDocumentId _documentId;
    private readonly string _errorMessage;
    private string _uploadFileName;
    public DocumentRedactionServiceTests()
    {
        var fixture = new Fixture();


        var mockRedactionProvider = new Mock<IRedactionProvider>();
        var mockLogger = new Mock<ILogger<DocumentRedactionService>>();

        _documentRedactionService = new DocumentRedactionService(
            mockRedactionProvider.Object,
            mockLogger.Object);

        _redactPdfRequest = fixture.Create<RedactPdfRequestDto>();
        _redactPdfRequestWithDocument = fixture.Create<RedactPdfRequestWithDocumentDto>();
        _redactPdfRequestWithDocument.Document = "base64string";
        _correlationId = fixture.Create<Guid>();
        _uploadFileName = fixture.Create<string>();
        _caseId = fixture.Create<string>();
        _documentId = fixture.Create<PolarisDocumentId>();

        var inputStream = new MemoryStream();
        var outputStream = new MemoryStream();

        mockRedactionProvider.Setup(s => s.Redact(
                It.Is<Stream>(s => s == inputStream),
                It.Is<string>(c => c == _caseId),
                It.Is<string>(d => d == _documentId.ToString()),
                It.Is<RedactPdfRequestDto>(r => r == _redactPdfRequest),
                It.Is<Guid>(g => g == _correlationId)
            ))
            .ReturnsAsync(outputStream);

        _errorMessage = fixture.Create<string>();
    }

    [Fact]
    public async Task DocumentRedactionService_RedactPdfAsync_ReturnsASuccessResponse()
    {
        // Act
        var saveResult = await _documentRedactionService.RedactAsync(_caseId, _documentId.ToString(), _redactPdfRequestWithDocument, _correlationId);

        // Assert
        using (new AssertionScope())
        {
            saveResult.Should().NotBeNull();
        }
    }
}