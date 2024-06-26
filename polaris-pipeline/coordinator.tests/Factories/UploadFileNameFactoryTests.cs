using FluentAssertions;
using coordinator.Factories.UploadFileNameFactory;
using Xunit;

namespace pdf_redactor.tests.Services.DocumentRedaction;

public class UploadFileNameFactoryTests
{
  private readonly IUploadFileNameFactory _uploadFileNameFactory;

  public UploadFileNameFactoryTests()
  {
    _uploadFileNameFactory = new UploadFileNameFactory();
  }

  [Theory]
  [InlineData("foo.pdf")]
  [InlineData("foo")]
  public void UploadFileNameFactory_BuildUploadFileName_ReturnsAFileNameWithAHash(string fileName)
  {
    // Act
    var uploadFileName = _uploadFileNameFactory.BuildUploadFileName(fileName);

    // Assert
    uploadFileName.Should().MatchRegex("foo_([0-9A-F]+).pdf");
  }

}