using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;
using pdf_generator.Functions;
using Common.Exceptions;

namespace pdf_generator.tests.Functions;

public class ConvertToPdfHelperTests
{
    private readonly IHeaderDictionary _headers;

    public ConvertToPdfHelperTests()
    {
        var httpContext = new DefaultHttpContext();
        _headers = httpContext.Request.Headers;
    }

    [Fact]
    public void WhenGettingFileType_AndTheHeaderDoesNotExist_AnExceptionIsThrown()
    {
        _headers.Append("BlobName", "BlobName");

        var act = () => ConvertToPdfHelper.GetFileType(_headers);

        act.Should().Throw<BadRequestException>()
            .WithMessage("Missing Filetype Value (Parameter 'headers')");
    }

    [Fact]
    public void WhenGettingFileType_AndTheHeaderValueIsEmpty_AnExceptionIsThrown()
    {
        var headerValue = "";
        _headers.Append(ConvertToPdfHelper.FiletypeKey, headerValue);

        var act = () => ConvertToPdfHelper.GetFileType(_headers);

        act.Should().Throw<BadRequestException>()
            .WithMessage("Missing Filetype Value (Parameter 'headers')");
    }

    [Fact]
    public void WhenGettingFileType_AndTheHeaderValueIsInvalid_AnExceptionIsThrown()
    {
        var headerValue = 99;
        _headers.Append(ConvertToPdfHelper.FiletypeKey, headerValue.ToString());

        var act = () => ConvertToPdfHelper.GetFileType(_headers);

        act.Should().Throw<BadRequestException>()
            .WithMessage($"Invalid Filetype Enum Value (Parameter '{headerValue}')");
    }

    [Fact]
    public void WhenGettingFileType_AndTheHeaderValueIsValid_AnEnumValueIsReturned()
    {
        var headerValue = Common.Domain.Document.FileType.PNG;

        _headers.Append(ConvertToPdfHelper.FiletypeKey, headerValue.ToString());

        var result = ConvertToPdfHelper.GetFileType(_headers);

        result.Should().Be(headerValue);
    }
}