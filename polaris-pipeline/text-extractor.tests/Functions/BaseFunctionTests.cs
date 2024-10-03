using System.Text.Json;
using Common.Converters;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using text_extractor.Functions;
using Xunit;

namespace text_extractor.tests.Functions;

public class BaseFunctionTests
{
    private class TestClass : BaseFunction
    {
        public JsonResult SurfacedCreateJsonResult(object result) => CreateJsonResult(result);
    }

    private readonly TestClass _testClass = new();

    [Fact]
    public void BaseFunction_CreateJsonResult_ResolvesJsonThatIsIndentedAndConvertsNamesToCamelCase()
    {
        // Arrange
        var jsonResult = _testClass.SurfacedCreateJsonResult(new { TestProperty = "TestValue" });

        // Act
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        var json = JsonSerializer.Serialize(jsonResult.Value, options);

        // Assert
        const string expectedValue = "\"testProperty\": \"TestValue\"";
        json.Should().Contain(expectedValue);
    }

    [Fact]
    public void BaseFunction_CreateJsonResult_ResolvesEmptyStringsAsNull()
    {
        // Arrange
        var jsonResult = _testClass.SurfacedCreateJsonResult(new { TestProperty = "" });

        // Act
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonNullToEmptyStringConverter() }
        };
        var json = JsonSerializer.Serialize(jsonResult.Value, options);

        // Assert
        const string expectedValue = "\"testProperty\": null";
        json.Should().Contain(expectedValue);
    }
}