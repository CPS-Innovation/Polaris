using Common.Validators;
using FluentAssertions;
using Xunit;

namespace Common.tests.Validators;

public class RequiredLongGreaterThanZeroTests
{
    [Theory]
    [InlineData("x", false)]
    [InlineData("1", true)]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void IsValid_ReturnsTheExpectedValue(object value, bool expectedResult)
    {
        var attribute = new RequiredLongGreaterThanZeroAttribute();

        var result = attribute.IsValid(value);

        result.Should().Be(expectedResult);
    }

    [Fact]
    public void IsValid_WhenWorkingWithMaxInteger_ReturnsTrue()
    {
        var attribute = new RequiredLongGreaterThanZeroAttribute();

        var result = attribute.IsValid(int.MaxValue);

        result.Should().BeTrue();
    }
    
    [Fact]
    public void IsValid_WhenWorkingWithMaxLong_ReturnsTrue()
    {
        var attribute = new RequiredLongGreaterThanZeroAttribute();

        var result = attribute.IsValid(long.MaxValue);

        result.Should().BeTrue();
    }
    
    [Fact]
    public void IsValid_WhenWorkingWithMaxDecimal_ReturnsFalse()
    {
        var attribute = new RequiredLongGreaterThanZeroAttribute();

        var result = attribute.IsValid(decimal.MaxValue);

        result.Should().BeFalse();
    }
}
