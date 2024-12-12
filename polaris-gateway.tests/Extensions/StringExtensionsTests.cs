using FluentAssertions;
using PolarisGateway.Extensions;
using Xunit;

namespace PolarisGateway.Tests.Extensions;

public class StringExtensionsTests
{
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

    [InlineData("CMSUSER123;", 123)]
    [InlineData("CMSUSER123;foo=bar", 123)]
    [InlineData("CMSUSER123; foo=bar", 123)]


    [InlineData("CMSUSER", 0)]
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
}
