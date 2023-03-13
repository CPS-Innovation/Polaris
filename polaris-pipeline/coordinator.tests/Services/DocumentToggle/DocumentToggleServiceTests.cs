using System;
using System.IO;
using coordinator.Domain.Tracker;
using coordinator.Domain.Tracker.PresentationStatus;
using coordinator.Services.DocumentToggle;
using coordinator.Services.DocumentToggle.Exceptions;
using FluentAssertions;
using Xunit;

namespace coordinator.tests.Services.DocumentToggle
{
    public class DocumentToggleServiceTests
    {
        private readonly DocumentToggleService _documentToggleService;
        public DocumentToggleServiceTests()
        {
            _documentToggleService = new DocumentToggleService();
        }

        [Fact]
        public void Static_LoadConfig_ReturnsDocumentToggleConfigFileContents()
        {
            // Act
            var content = DocumentToggleService.LoadConfig();

            // Assert
            content.Should().Be(File.ReadAllText("document-toggle.config"));
        }

        [Fact]
        public void Init_ShouldThrowForNullContent()
        {
            Assert.Throws<ArgumentNullException>(() => _documentToggleService.Init(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData(" \n")]
        [InlineData(" \n \n")]
        [InlineData(" \n \t\n   \n")]
        public void Init_ShouldAcceptEmptyContent(string content)
        {
            // Act
            var exception = Record.Exception(() => _documentToggleService.Init(content));

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("#foo")]
        [InlineData("#foo\n#bar")]
        public void Init_ShouldAcceptEmptyContentWithComments(string content)
        {
            // Act
            var exception = Record.Exception(() => _documentToggleService.Init(content));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Init_ShouldThrowForRepeatedCallToInit()
        {
            // Arrange
            var exception = Record.Exception(() => _documentToggleService.Init(""));
            Assert.Null(exception);

            // Assert
            Assert.Throws<DocumentToggleException>(() => _documentToggleService.Init(""));
        }

        [Theory]
        [InlineData(
          @"",
          ".pdf", "MG1", ReadStatus.OnlyAvailableInCms, WriteStatus.OnlyAvailableInCms)]
        [InlineData(
          @"FileType Read *",
          ".pdf", "MG1", ReadStatus.OnlyAvailableInCms, WriteStatus.OnlyAvailableInCms)]
        [InlineData(
          @"DocType Read *",
          ".pdf", "MG1", ReadStatus.OnlyAvailableInCms, WriteStatus.OnlyAvailableInCms)]
        [InlineData(
          @"FileType  Read *
            DocType   Read *",
          ".pdf", "MG1", ReadStatus.Ok, WriteStatus.DocTypeNotAllowed)]
        [InlineData(
          @"FileType  ReadWrite *
            DocType   Read      *",
          ".pdf", "MG1", ReadStatus.Ok, WriteStatus.DocTypeNotAllowed)]
        [InlineData(
          @"FileType  Read      *
            DocType   ReadWrite *",
          ".pdf", "MG1", ReadStatus.Ok, WriteStatus.OriginalFileTypeNotAllowed)]
        [InlineData(
          @"FileType  ReadWrite *
            DocType   ReadWrite *",
          ".pdf", "MG1", ReadStatus.Ok, WriteStatus.Ok)]
        [InlineData(
          @"FileType  ReadWrite *
            DocType   ReadWrite *",
          ".pdf", "MG1", ReadStatus.Ok, WriteStatus.Ok)]
        [InlineData(
          @"FileType  ReadWrite .pdf
            DocType   ReadWrite MG1",
          ".pdf", "MG1", ReadStatus.Ok, WriteStatus.Ok)]
        [InlineData(
          @"FileType  ReadWrite .doc
            DocType   ReadWrite MG2",
          ".pdf", "MG1", ReadStatus.OnlyAvailableInCms, WriteStatus.OnlyAvailableInCms)]
        [InlineData(
          @"FileType  ReadWrite *
            FileType  Deny      .pdf
            DocType   ReadWrite *",
          ".pdf", "MG1", ReadStatus.OnlyAvailableInCms, WriteStatus.OnlyAvailableInCms)]
        [InlineData(
          @"FileType  ReadWrite *
            FileType  Read      .pdf
            DocType   ReadWrite *",
          ".pdf", "MG1", ReadStatus.Ok, WriteStatus.OriginalFileTypeNotAllowed)]
        [InlineData(
          @"FileType  ReadWrite *
            #FileType  Read      .pdf
            DocType   ReadWrite *",
          ".pdf", "MG1", ReadStatus.Ok, WriteStatus.Ok)]
        public void SetDocumentPresentationStatuses_ShouldObeyTheRules(string configContent,
                                                                       string inputDocumentExtension,
                                                                       string inputDocumentCmsType,
                                                                       ReadStatus expectedReadStatus,
                                                                       WriteStatus expectWriteStatus)
        {
            // Arrage
            _documentToggleService.Init(configContent);

            var document = new TrackerDocument();
            document.CmsDocumentExtension = inputDocumentExtension;
            document.CmsDocType.DocumentType = inputDocumentCmsType;

            // Act
            _documentToggleService.SetDocumentPresentationStatuses(document);

            // Assert
            document.PresentationStatuses.ReadStatus.Should().Be(expectedReadStatus);
            document.PresentationStatuses.WriteStatus.Should().Be(expectWriteStatus);
        }
    }
}