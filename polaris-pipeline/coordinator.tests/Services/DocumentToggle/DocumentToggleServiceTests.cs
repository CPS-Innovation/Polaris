using System;
using System.IO;
using Common.Domain.Entity;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;
using Common.ValueObjects;
using coordinator.Services.DocumentToggle;
using FluentAssertions;
using Xunit;

namespace coordinator.tests.Services.DocumentToggle
{
  // todo: this suite ripe for a refactor!
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
      var document = new CmsDocumentEntity(
          new PolarisDocumentId("DOC-ID"),
          1,
          "2",
          3L,
          new DocumentTypeDto(),
          "foo/bar.pdf",
          "pdf",
          "fileCreated",
          "filename",
          "title",
          true,
          false,
          2,
          new PolarisDocumentId("3"),
          "3",
          null,
          new PresentationFlagsDto());
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
      var document = new CmsDocumentEntity(
          new PolarisDocumentId("DOC-ID"),
          1,
          "2",
          3L,
          new DocumentTypeDto(),
          "foo/bar.pdf",
          "pdf",
          "fileCreated",
          "filename",
          "title",
          true,
          false,
          2,
          new PolarisDocumentId("3"),
          "3",
          null,
          new PresentationFlagsDto());
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
      var document = new CmsDocumentEntity(
          new PolarisDocumentId("DOC-ID"),
          1,
          "2",
          3L,
          new DocumentTypeDto(),
          "foo/bar.pdf",
          "pdf",
          "fileCreated",
          "filename",
          "title",
          true,
          false,
          2,
          new PolarisDocumentId("3"),
          "3",
          null,
          new PresentationFlagsDto());
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
      var document = new CmsDocumentEntity(
          new PolarisDocumentId("DOC-ID"),
          1,
          "2",
          3L,
          new DocumentTypeDto(),
          "foo/bar.pdf",
          "pdf",
          "fileCreated",
          "filename",
          "title",
          true,
          false,
          2,
          new PolarisDocumentId("3"),
          "3",
          null,
          new PresentationFlagsDto());
      document.PresentationFlags.Write = WriteFlag.Ok;

      // Assert
      var canWrite = documentToggleService.CanWriteDocument(document);
      canWrite.Should().BeTrue();
    }

    [Fact]
    public void GetDocumentPresentationFlags_ReturnsIsDispatched_IfDocumentIsDispatched()
    {
      // Arrange	
      var documentToggleService = new DocumentToggleService("FileType ReadWrite *\nDocType ReadWrite *");
      var document = new CmsDocumentDto
      {
        CmsDocType = new DocumentTypeDto(),
        IsDispatched = true
      };

      //Act	
      var presentationFlags = documentToggleService.GetDocumentPresentationFlags(document);

      // Assert	
      presentationFlags.Write.Should().Be(WriteFlag.IsDispatched);
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
      // Arrange
      var documentToggleService = new DocumentToggleService(configContent);

      var document = new CmsDocumentDto
      {
        FileExtension = inputDocumentExtension,
        CmsDocType = new DocumentTypeDto
        {
          DocumentTypeId = inputDocumentCmsType
        },
      };

      // Act
      var presentationFlags = documentToggleService.GetDocumentPresentationFlags(document);

      // Assert
      presentationFlags.Read.Should().Be(expectedReadFlag);
      presentationFlags.Write.Should().Be(expectWriteFlag);
    }

    [Theory]
    [InlineData(".doc", "0", true, WriteFlag.Ok)]
    [InlineData(".docm", "0", true, WriteFlag.Ok)]
    [InlineData(".docx", "0", true, WriteFlag.Ok)]
    [InlineData(".pdf", "0", true, WriteFlag.Ok)]
    [InlineData(".bmp", "0", true, WriteFlag.Ok)]
    [InlineData(".gif", "0", true, WriteFlag.Ok)]
    [InlineData(".jpeg", "0", true, WriteFlag.Ok)]
    [InlineData(".jpg", "0", true, WriteFlag.Ok)]
    [InlineData(".png", "0", true, WriteFlag.Ok)]
    [InlineData(".ppt", "0", true, WriteFlag.Ok)]
    [InlineData(".pptx", "0", true, WriteFlag.Ok)]
    [InlineData(".rtf", "0", true, WriteFlag.Ok)]
    [InlineData(".text", "0", true, WriteFlag.Ok)]
    [InlineData(".tif", "0", true, WriteFlag.Ok)]
    [InlineData(".tiff", "0", true, WriteFlag.Ok)]
    [InlineData(".txt", "0", true, WriteFlag.Ok)]
    [InlineData(".xls", "0", true, WriteFlag.Ok)]
    [InlineData(".xlsx", "0", true, WriteFlag.Ok)]
    [InlineData(".hte", "0", true, WriteFlag.OriginalFileTypeNotAllowed)]
    [InlineData(".doc", "0", false, WriteFlag.IsNotOcrProcessed)]
    [InlineData(".doc", "-54321", true, WriteFlag.DocTypeNotAllowed)]
    public void CurrentRulesInDocumentToggleFile_WorkAsExpected(string filetype, string docTypeId, bool isOcrProcessed, WriteFlag writeFlag)
    {
      // Arrange
      var documentToggleService = new DocumentToggleService(DocumentToggleService.ReadConfig());

      var document = new CmsDocumentDto
      {
        FileExtension = filetype,
        CmsDocType = new DocumentTypeDto
        {
          DocumentTypeId = docTypeId
        },
        IsOcrProcessed = isOcrProcessed
      };

      // Act
      var presentationFlags = documentToggleService.GetDocumentPresentationFlags(document);

      // Assert
      presentationFlags.Read.Should().Be(ReadFlag.Ok);
      presentationFlags.Write.Should().Be(writeFlag);
    }
  }
}