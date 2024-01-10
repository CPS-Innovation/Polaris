using polaris_common.Domain.Document;
using polaris_common.Domain.Exceptions;
using FluentAssertions;
using polaris_common.Domain.Extensions;
using Xunit;

namespace pdf_generator.tests
{
	public class ExtensionsTests
	{
		[Theory]
		[InlineData("doc", FileType.DOC)]
		[InlineData("DoC", FileType.DOC)]
		[InlineData("docx", FileType.DOCX)]
		[InlineData("docm", FileType.DOCM)]
		[InlineData("rtf", FileType.RTF)]
		[InlineData("txt", FileType.TXT)]
		[InlineData("xls", FileType.XLS)]
		[InlineData("xlsx", FileType.XLSX)]
		[InlineData("ppt", FileType.PPT)]
		[InlineData("pptx", FileType.PPTX)]
		[InlineData("pdf", FileType.PDF)]
		[InlineData("png", FileType.PNG)]
		[InlineData("jpg", FileType.JPG)]
		[InlineData("jpeg", FileType.JPEG)]
		[InlineData("bmp", FileType.BMP)]
		[InlineData("gif", FileType.GIF)]
		[InlineData("tif", FileType.TIF)]
		[InlineData("tiff", FileType.TIFF)]
		[InlineData("vsd", FileType.VSD)]
		[InlineData("html", FileType.HTML)]
		[InlineData("htm", FileType.HTM)]
		[InlineData("msg", FileType.MSG)]
		public void ToFileType_ReturnsExpectedFileType(string fileType, FileType fileTypeEnum)
		{
			var fileTypeValue = fileType.ToFileType();

			fileTypeValue.Should().Be(fileTypeEnum);
		}

		[Fact]
		public void ToFileType_ThrowsWhenFileTypeIsInteger()
		{
			var fileType = "6";
			Assert.Throws<UnsupportedFileTypeException>(() => fileType.ToFileType());
		}

		[Fact]
		public void ToFileType_ThrowsWhenUnsupportedFileType()
		{
			var fileType = "Unsupported file type";
			Assert.Throws<UnsupportedFileTypeException>(() => fileType.ToFileType());
		}
	}
}

