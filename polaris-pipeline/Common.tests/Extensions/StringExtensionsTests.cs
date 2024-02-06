using Common.Domain.Extensions;
using Common.Extensions;
using FluentAssertions;
using Xunit;

namespace Common.tests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UrlEncodeString_IfNullOrEmptyString_ReturnsEmptyString(string valueIn)
    {
        var convertedItem = valueIn.UrlEncodeString();

        convertedItem.Should().BeEmpty();
    }

    [Fact]
    public void UrlEncodeString_UrlEncodesAString_AsExpected()
    {
        const string itemToTest = "A TEST DOC.pdf";
        const string itemToMatch = "A%20TEST%20DOC.pdf";
        var convertedItem = itemToTest.UrlEncodeString();

        convertedItem.Should().Be(itemToMatch);
    }

    [Fact]
    public void UrlEncodeString_DoesNotChange_ANoneConvertibleString_AsExpected()
    {
        const string itemToTest = "james-crane.pdf";
        const string itemToMatch = "james-crane.pdf";
        var convertedItem = itemToTest.UrlEncodeString();

        convertedItem.Should().Be(itemToMatch);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UrlDecodeString_IfNullOrEmptyString_ReturnsEmptyString(string valueIn)
    {
        var convertedItem = valueIn.UrlDecodeString();

        convertedItem.Should().BeEmpty();
    }

    [Fact]
    public void UrlDecodeString_UrlEncodesAString_AsExpected()
    {
        const string itemToTest = "18846%2Fpdfs%2FMCLOVE%20MG3.pdf";
        const string itemToMatch = "18846/pdfs/MCLOVE MG3.pdf";
        var convertedItem = itemToTest.UrlDecodeString();

        convertedItem.Should().Be(itemToMatch);
    }

    [Fact]
    public void UrlDecodeString_DoesNotChange_ANoneConvertibleString_AsExpected()
    {
        const string itemToTest = "james%2F-%20%26crane.pdf";
        const string itemToMatch = "james/- &crane.pdf";
        var convertedItem = itemToTest.UrlDecodeString();

        convertedItem.Should().Be(itemToMatch);
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData(" ", 0)]

    [InlineData("UID=123", 123)]
    [InlineData("UID=123;", 123)]
    [InlineData("UID=123;foo=bar", 123)]
    [InlineData("UID=123; foo=bar", 123)]
    [InlineData("foo=bar;UID=123", 123)]
    [InlineData("foo=bar;UID=123;", 123)]
    [InlineData("foo=bar;UID=123; foo=bar", 123)]
    [InlineData("foo=bar; UID=123; foo=bar", 123)]

    [InlineData("UID=", 0)]
    [InlineData("UID=;", 0)]
    [InlineData("UID=;foo=bar", 0)]
    [InlineData("UID=; foo=bar", 0)]
    [InlineData("foo=bar;UID=", 0)]
    [InlineData("foo=bar;UID=;", 0)]
    public void ExtractCmsUserId_ExtractsCmsUserId(string valueIn, long expectedResult)
    {
        var result = valueIn.ExtractCmsUserId();

        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("foo=bar", "")]
    [InlineData("BIGipServerabcd=1234", "BIGipServerabcd=1234")]
    [InlineData("BIGipServerabcd=1234;", "BIGipServerabcd=1234")]
    [InlineData("BIGipServerabcd=1234; foo=bar", "BIGipServerabcd=1234")]
    [InlineData("foo=bar; BIGipServerabcd=1234", "BIGipServerabcd=1234")]
    [InlineData("foo=bar; BIGipServerabcd=1234;", "BIGipServerabcd=1234")]
    [InlineData("foo=bar; BIGipServerabcd=1234; baz=qux", "BIGipServerabcd=1234")]
    [InlineData("BIGipServerabcd=1234; BIGipServerabcd=5678", "BIGipServerabcd=1234; BIGipServerabcd=5678")]
    [InlineData("BIGipServerabcd=1234; BIGipServerabcd=5678;", "BIGipServerabcd=1234; BIGipServerabcd=5678")]
    [InlineData("foo=bar; BIGipServerabcd=1234; baz=qux; BIGipServerabcd=5678", "BIGipServerabcd=1234; BIGipServerabcd=5678")]
    [InlineData("foo=bar; BIGipServerabcd=1234; baz=qux; BIGipServerabcd=5678; foo=bar;", "BIGipServerabcd=1234; BIGipServerabcd=5678")]

    public void ExtractLoadBalancerCookie_ExtractsCookie(string valueIn, string expectedResult)
    {
        var result = valueIn.ExtractLoadBalancerCookies();

        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("[12345]", "[", "]", "12345")]
    [InlineData("[2149310]-CMS-4267077", "[", "]", "2149310")]
    public void ExtractBookendedContent_ReturnsExpectedStringValue(string valueIn, string from, string until,
        string expectedContent)
    {
        var result = valueIn.ExtractBookendedContent(from, until);
        result.Should().Be(expectedContent);
    }

    [Fact]
    public void ExtractBookendedContent_ThrowsExceptionIfFirstBookendIsMissing()
    {
        const string testValue = "2149310-CMS-4267077";
        Assert.Throws<ArgumentException>(() => testValue.ExtractBookendedContent("[", "]"));
    }
    
    [Fact]
    public void ExtractBookendedContent_ThrowsExceptionIfSecondBookendIsMissing()
    {
        const string testValue = "[2149310-CMS-4267077";
        Assert.Throws<ArgumentException>(() => testValue.ExtractBookendedContent("[", "]"));
    }
}
