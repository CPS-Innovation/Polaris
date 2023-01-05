using Common.Domain.Extensions;
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
}
