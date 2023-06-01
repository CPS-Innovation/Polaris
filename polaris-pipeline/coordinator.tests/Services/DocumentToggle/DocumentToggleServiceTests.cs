using System;
using System.IO;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;
using Common.Dto.Tracker;
using Common.Services.DocumentToggle;
using Common.ValueObjects;
using FluentAssertions;
using Xunit;

namespace coordinator.tests.Services.DocumentToggle
{
    public class DocumentToggleServiceTests
    {
        [Fact]
        public void Static_LoadConfig_ReturnsDocumentToggleConfigFileContents()
        {
            // Act
            var content = DocumentToggleService.ReadConfig();

            // Assert
            content.Should().Be(File.ReadAllText("document-toggle.config"));
        }

        [Fact]
        public void CurrentPackagedConfigFile_DoesNotCauseException()
        {
            // Act
            var exception = Record.Exception(() => new DocumentToggleService(
              DocumentToggleService.ReadConfig())
            );

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_ShouldThrowForNullContent()
        {
            Assert.Throws<ArgumentNullException>(() => new DocumentToggleService(null));
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
            var exception = Record.Exception(() => new DocumentToggleService(content));

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [InlineData("#foo")]
        [InlineData("#foo\n#bar")]
        public void Constructor_ShouldAcceptEmptyContentWithComments(string content)
        {
            // Act
            var exception = Record.Exception(() => new DocumentToggleService(content));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void CanReadDocument_ReturnsFalseIfDocumentNotReadable()
        {
            // Arrange
            var documentToggleService = new DocumentToggleService("");
            var document = new TrackerCmsDocumentDto(new PolarisDocumentId("DOC-ID"), 1, "2", 3L, new DocumentTypeDto(), "fileCreated", "filename", "title", true, new PresentationFlagsDto());
            document.PresentationFlags.Read = ReadFlag.OnlyAvailableInCms;

            // Assert
            var canRead = documentToggleService.CanReadDocument(document);
            canRead.Should().BeFalse();
        }

        [Fact]
        public void CanReadDocument_ReturnsTrueIfDocumentReadable()
        {
            // Arrange
            var documentToggleService = new DocumentToggleService("");
            var document = new TrackerCmsDocumentDto(new PolarisDocumentId("DOC-ID"), 1, "2", 3L, new DocumentTypeDto(), "fileCreated", "filename", "title", true, new PresentationFlagsDto());
            document.PresentationFlags.Read = ReadFlag.Ok;

            // Assert
            var canRead = documentToggleService.CanReadDocument(document);
            canRead.Should().BeTrue();
        }

        [Fact]
        public void CanWriteDocument_ReturnsFalseIfDocumentNotWriteable()
        {
            // Arrange
            var documentToggleService = new DocumentToggleService("");
            var document = new TrackerCmsDocumentDto(new PolarisDocumentId("DOC-ID"), 1, "2", 3L, new DocumentTypeDto(), "fileCreated", "filename", "title", true, new PresentationFlagsDto());
            document.PresentationFlags.Write = WriteFlag.OnlyAvailableInCms;


            // Assert
            var canWrite = documentToggleService.CanWriteDocument(document);
            canWrite.Should().BeFalse();
        }

        [Fact]
        public void CanWriteDocument_ReturnsTrueIfDocumentWriteable()
        {
            // Arrange
            var documentToggleService = new DocumentToggleService("");
            var document = new TrackerCmsDocumentDto(new PolarisDocumentId("DOC-ID"), 1, "2", 3L, new DocumentTypeDto(), "fileCreated", "filename", "title", true, new PresentationFlagsDto());
            document.PresentationFlags.Write = WriteFlag.Ok;

            // Assert
            var canWrite = documentToggleService.CanWriteDocument(document);
            canWrite.Should().BeTrue();
        }

        [Theory]
        [InlineData(
          @"",
          ".pdf", "MG1", ReadFlag.OnlyAvailableInCms, WriteFlag.OnlyAvailableInCms)]
        [InlineData(
          @"FileType Read *",
          ".pdf", "MG1", ReadFlag.OnlyAvailableInCms, WriteFlag.OnlyAvailableInCms)]
        [InlineData(
          @"DocType Read *",
          ".pdf", "MG1", ReadFlag.OnlyAvailableInCms, WriteFlag.OnlyAvailableInCms)]
        [InlineData(
          @"FileType  Read *
            DocType   Read *",
          ".pdf", "MG1", ReadFlag.Ok, WriteFlag.DocTypeNotAllowed)]
        [InlineData(
          @"FileType  ReadWrite *
            DocType   Read      *",
          ".pdf", "MG1", ReadFlag.Ok, WriteFlag.DocTypeNotAllowed)]
        [InlineData(
          @"FileType  Read      *
            DocType   ReadWrite *",
          ".pdf", "MG1", ReadFlag.Ok, WriteFlag.OriginalFileTypeNotAllowed)]
        [InlineData(
          @"FileType  ReadWrite *
            DocType   ReadWrite *",
          ".pdf", "MG1", ReadFlag.Ok, WriteFlag.IsNotOcrProcessed)]
        [InlineData(
          @"FileType  ReadWrite .pdf
            DocType   ReadWrite MG1",
          ".pdf", "MG1", ReadFlag.Ok, WriteFlag.IsNotOcrProcessed)]
        [InlineData(
          @"FileType  ReadWrite .doc
            DocType   ReadWrite MG2",
          ".pdf", "MG1", ReadFlag.OnlyAvailableInCms, WriteFlag.OnlyAvailableInCms)]
        [InlineData(
          @"FileType  ReadWrite *
            FileType  Deny      .pdf
            DocType   ReadWrite *",
          ".pdf", "MG1", ReadFlag.OnlyAvailableInCms, WriteFlag.OnlyAvailableInCms)]
        [InlineData(
          @"FileType  ReadWrite *
            FileType  Read      .pdf
            DocType   ReadWrite *",
          ".pdf", "MG1", ReadFlag.Ok, WriteFlag.OriginalFileTypeNotAllowed)]
        [InlineData(
          @"FileType  ReadWrite *
            #FileType  Read      .pdf
            DocType   ReadWrite *",
          ".pdf", "MG1", ReadFlag.Ok, WriteFlag.IsNotOcrProcessed)]
        public void SetDocumentPresentationFlags_ShouldObeyTheRules(string configContent,
                                                                       string inputDocumentExtension,
                                                                       string inputDocumentCmsType,
                                                                       ReadFlag expectedReadFlag,
                                                                       WriteFlag expectWriteFlag)
        {
            // Arrage
            var documentToggleService = new DocumentToggleService(configContent);

            var document = new DocumentDto();
            document.FileExtension = inputDocumentExtension;
            document.CmsDocType = new DocumentTypeDto();
            document.CmsDocType.DocumentType = inputDocumentCmsType;

            // Act
            var presentationFlags = documentToggleService.GetDocumentPresentationFlags(document);

            // Assert
            presentationFlags.Read.Should().Be(expectedReadFlag);
            presentationFlags.Write.Should().Be(expectWriteFlag);
        }
    }
}